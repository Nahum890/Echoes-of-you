using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

/// <summary>
/// State machine para un botón de menú individual dentro del sistema de hover icónico.
/// Gestiona: ghost layers, aberración cromática, ruido, eco visual, partículas, 
/// vibración analógica, scan line y el pulso orgánico de los efectos en hover held.
///
/// Creado por MenuHoverSystem.cs — uno por cada botón registrado.
/// </summary>
public class MenuButtonHoverState
{
    // ═══════════════════════════════════════════════════════════════
    // REFERENCIAS
    // ═══════════════════════════════════════════════════════════════

    readonly Button _button;
    readonly MenuHoverSystem _system;
    readonly string _labelText;
    public PanelTintType TintType { get; }

    // Capas de efecto (VisualElements hijos del botón)
    VisualElement _ghostA;
    VisualElement _ghostB;
    VisualElement _chromaR;
    VisualElement _chromaB;
    VisualElement _noiseOverlay;
    VisualElement _echoLayer;
    VisualElement _focusPip;
    VisualElement _confirmFlash;
    VisualElement _scanLine;

    // Pool de partículas
    readonly List<UIParticleInstance> _activeParticles = new();

    // ═══════════════════════════════════════════════════════════════
    // ESTADO DE LA MÁQUINA
    // ═══════════════════════════════════════════════════════════════

    enum State { Idle, HoverEnter, HoverHeld, HoverLeave, Pressed }
    State _currentState = State.Idle;

    // Corrutinas activas
    Coroutine _hoverHeldLoop;
    Coroutine _noiseLoop;
    Coroutine _analogJitterLoop;
    Coroutine _scanLineLoop;
    Coroutine _echoCoroutine;

    // Valores internos para el pulso orgánico
    float _ghostAPulsePhase;
    float _ghostBPulsePhase;
    float _noisePulsePhase;

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCTOR
    // ═══════════════════════════════════════════════════════════════

    public MenuButtonHoverState(Button button, MenuHoverSystem system, string labelText, PanelTintType tintType)
    {
        _button    = button;
        _system    = system;
        _labelText = labelText;
        TintType   = tintType;

        BuildEffectLayers();
    }

    // ═══════════════════════════════════════════════════════════════
    // CONSTRUCCIÓN DE CAPAS DE EFECTO
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Construye todos los VisualElements de efectos e inyecta en el DOM
    /// como hermanos del botón (dentro de su wrapper) o como hijos directos.
    /// </summary>
    void BuildEffectLayers()
    {
        VisualElement parent = _button.parent ?? _button;

        // Ghost Layer A — eco primario
        _ghostA = new Label(_labelText);
        _ghostA.AddToClassList("nav-item-ghost-a");
        parent.Insert(parent.IndexOf(_button), _ghostA);

        // Ghost Layer B — eco secundario (más profundo)
        _ghostB = new Label(_labelText);
        _ghostB.AddToClassList("nav-item-ghost-b");
        parent.Insert(parent.IndexOf(_button), _ghostB);

        // Aberración cromática — Canal R
        _chromaR = new Label(_labelText);
        _chromaR.AddToClassList("nav-item-chroma-r");
        _button.Add(_chromaR);

        // Aberración cromática — Canal B
        _chromaB = new Label(_labelText);
        _chromaB.AddToClassList("nav-item-chroma-b");
        _button.Add(_chromaB);

        // Overlay de ruido analógico
        _noiseOverlay = new VisualElement();
        _noiseOverlay.AddToClassList("nav-item-noise");
        _button.Add(_noiseOverlay);

        // Eco visual — se dispara una vez al entrar en hover
        _echoLayer = new Label(_labelText);
        _echoLayer.AddToClassList("nav-item-echo");
        _button.Add(_echoLayer);

        // Focus pip — indicador de foco para controller
        _focusPip = new Label("▸");
        _focusPip.AddToClassList("nav-item-focus-pip");
        _button.Add(_focusPip);

        // Flash de confirmación al hacer click
        _confirmFlash = new VisualElement();
        _confirmFlash.AddToClassList("nav-item-confirm-flash");
        _button.Add(_confirmFlash);

        // Scan line analógica
        _scanLine = new VisualElement();
        _scanLine.AddToClassList("nav-item-scanline");
        _button.Add(_scanLine);
    }

    // ═══════════════════════════════════════════════════════════════
    // TRANSICIONES DE ESTADO
    // ═══════════════════════════════════════════════════════════════

    /// <summary>
    /// Activa el estado hover con todos sus efectos en cascada.
    /// </summary>
    public void EnterHover(float duration, bool isController)
    {
        if (_currentState == State.HoverHeld) return;

        _currentState = State.HoverEnter;
        StopAllLoops();

        // La clase :hover de USS maneja: escala, translate, color de texto, border
        // Desde C# activamos las capas que USS no puede animar solo:

        // 1. Ghost layers — con delay para el efecto de cascada analógica
        _system.RunCoroutine(ActivateGhostLayers());

        // 2. Aberración cromática — con micro-delay
        _system.RunCoroutine(ActivateChromaLayers());

        // 3. Ruido — aparece un poco después
        _system.RunCoroutine(ActivateNoise());

        // 4. Eco visual — one-shot
        _echoCoroutine = _system.RunCoroutine(FireEchoLayer());

        // 5. Vibración de entrada — micro-shake de 3 frames
        _system.RunCoroutine(VibrationShake());

        // 6. Scan line
        _scanLineLoop = _system.RunCoroutine(RunScanLine());

        // 7. Partículas
        _system.RunCoroutine(SpawnHoverParticles());

        // 8. Focus pip (solo controller)
        if (isController)
            _focusPip.AddToClassList("nav-item-focus-pip--visible");

        // 9. Blur simulado — reduce opacidad del texto brevemente, luego sube
        _system.RunCoroutine(SimulateBlur());

        // Después de la entrada, pasar a held
        _system.RunCoroutine(TransitionToHeld(duration));
    }

    /// <summary>
    /// Desactiva todos los efectos y vuelve al estado idle.
    /// </summary>
    public void ExitHover(float duration)
    {
        if (_currentState == State.Idle) return;

        _currentState = State.HoverLeave;
        StopAllLoops();

        // Ghost layers salen primero
        _ghostA.RemoveFromClassList("nav-item-ghost-a--visible");
        _ghostB.RemoveFromClassList("nav-item-ghost-b--visible");

        // Chroma ligeramente después
        _system.RunCoroutine(DeactivateWithDelay(_chromaR, "nav-item-chroma-r--visible", 0.01f));
        _system.RunCoroutine(DeactivateWithDelay(_chromaB, "nav-item-chroma-b--visible", 0.01f));

        // Ruido
        _noiseOverlay.RemoveFromClassList("nav-item-noise--visible");
        _system.RunCoroutine(FadeNoiseOut());

        // Focus pip
        _focusPip.RemoveFromClassList("nav-item-focus-pip--visible");

        // Marcar como idle después de la transición
        _system.RunCoroutine(SetIdleAfterDelay(duration));
    }

    /// <summary>
    /// Efecto de presionar: escala hacia abajo + flash ámbar + chroma intensificado.
    /// </summary>
    public void TriggerPress()
    {
        _currentState = State.Pressed;
        _system.RunCoroutine(PressSequence());
    }

    // ═══════════════════════════════════════════════════════════════
    // CORRUTINAS DE EFECTOS
    // ═══════════════════════════════════════════════════════════════

    IEnumerator TransitionToHeld(float duration)
    {
        yield return new WaitForSecondsRealtime(duration);
        if (_currentState == State.HoverEnter)
        {
            _currentState = State.HoverHeld;
            _hoverHeldLoop = _system.RunCoroutine(HoverHeldLoop());
            _noiseLoop = _system.RunCoroutine(NoisePulseLoop());
            _analogJitterLoop = _system.RunCoroutine(AnalogJitterLoop());
        }
    }

    /// <summary>
    /// Bucle de hover held: pulso orgánico de ghost layers con ondas senoidales desfasadas.
    /// </summary>
    IEnumerator HoverHeldLoop()
    {
        _ghostAPulsePhase = 0f;
        _ghostBPulsePhase = 0.6f; // Desfase para que no estén sincronizados

        float ghostAPeriod = 1.8f;
        float ghostBPeriod = 2.2f;
        float ghostAMin = _system.GhostLayerAOpacity * 0.85f;
        float ghostAMax = _system.GhostLayerAOpacity * 1.22f;
        float ghostBMin = _system.GhostLayerBOpacity * 0.85f;
        float ghostBMax = _system.GhostLayerBOpacity * 1.22f;

        while (_currentState == State.HoverHeld)
        {
            _ghostAPulsePhase += Time.unscaledDeltaTime / ghostAPeriod * (2f * Mathf.PI);
            _ghostBPulsePhase += Time.unscaledDeltaTime / ghostBPeriod * (2f * Mathf.PI);

            float aOpacity = Mathf.Lerp(ghostAMin, ghostAMax, (Mathf.Sin(_ghostAPulsePhase) + 1f) * 0.5f);
            float bOpacity = Mathf.Lerp(ghostBMin, ghostBMax, (Mathf.Sin(_ghostBPulsePhase) + 1f) * 0.5f);

            _ghostA.style.color = new Color(0.788f, 0.831f, 0.690f, aOpacity);
            _ghostB.style.color = new Color(0.788f, 0.831f, 0.690f, bOpacity);

            yield return null;
        }
    }

    /// <summary>
    /// Pulso aleatorio de la opacidad del ruido — no es periódico, es orgánico.
    /// </summary>
    IEnumerator NoisePulseLoop()
    {
        while (_currentState == State.HoverHeld)
        {
            float targetOpacity = Random.Range(0.04f, _system.NoiseBaseOpacity * 1.4f);
            float duration      = Random.Range(0.4f, 1.2f);
            float startOpacity  = _noiseOverlay.style.opacity.value;
            float elapsed       = 0f;

            while (elapsed < duration && _currentState == State.HoverHeld)
            {
                elapsed += Time.unscaledDeltaTime;
                _noiseOverlay.style.opacity = Mathf.Lerp(startOpacity, targetOpacity, elapsed / duration);
                yield return null;
            }

            // Espera orgánica antes del siguiente impulso
            yield return new WaitForSecondsRealtime(Random.Range(0.1f, 0.6f));
        }
    }

    /// <summary>
    /// Micro-jitter analógico: impulsos de sub-pixel que simulan una señal inestable.
    /// </summary>
    IEnumerator AnalogJitterLoop()
    {
        while (_currentState == State.HoverHeld)
        {
            // Esperar tiempo aleatorio entre impulsos
            yield return new WaitForSecondsRealtime(Random.Range(0.8f, 2.4f));

            if (_currentState != State.HoverHeld) break;

            // Aplicar micro-impulso — 1 frame
            float jitter = Random.Range(-0.5f, 0.5f);
            _button.style.translate = new StyleTranslate(new Translate(Length.Percent(0), Length.Percent(0)));
            // Nota: el translate real viene de la clase :hover en USS.
            // Este jitter se aplica como marginLeft temporal:
            _button.style.marginLeft = jitter;
            yield return null;
            _button.style.marginLeft = 0f;
        }
    }

    /// <summary>
    /// Scan line analógica: una línea fina que recorre el botón.
    /// </summary>
    IEnumerator RunScanLine()
    {
        float btnHeight = _button.resolvedStyle.height;
        if (btnHeight < 1f) btnHeight = 36f; // fallback

        _scanLine.style.opacity = 0.15f;

        while (_currentState == State.HoverEnter || _currentState == State.HoverHeld)
        {
            float speed = Random.Range(60f, 120f); // px/s
            float t = 0f;
            float duration = btnHeight / speed;

            _scanLine.style.top = 0f;

            while (t < duration)
            {
                t += Time.unscaledDeltaTime;
                _scanLine.style.top = Mathf.Lerp(0f, btnHeight, t / duration);
                yield return null;
            }

            // Pausa entre barridos — aleatoria y poco frecuente
            _scanLine.style.opacity = 0f;
            yield return new WaitForSecondsRealtime(Random.Range(2f, 6f));
            _scanLine.style.opacity = 0.15f;
        }

        _scanLine.style.opacity = 0f;
    }

    /// <summary>
    /// Dispara el eco visual: copia del texto que se desvanece hacia la derecha.
    /// Se dispara UNA SOLA VEZ al entrar en hover.
    /// </summary>
    IEnumerator FireEchoLayer()
    {
        // Reset al estado inicial
        _echoLayer.RemoveFromClassList("nav-item-echo--fired");
        _echoLayer.style.opacity = 0.4f;

        // Espera un frame para que los estilos se apliquen
        yield return null;

        // Disparar la animación CSS
        _echoLayer.AddToClassList("nav-item-echo--fired");

        // Limpiar después de que termine la animación
        yield return new WaitForSecondsRealtime(0.12f);
        _echoLayer.RemoveFromClassList("nav-item-echo--fired");
        _echoLayer.style.opacity = 0f;
    }

    /// <summary>
    /// Micro-shake de 3 frames al entrar en hover — vibración analógica.
    /// </summary>
    IEnumerator VibrationShake()
    {
        // Frame 1: desplazamiento máximo + ligero Y negativo
        _button.style.marginTop = -0.5f;
        _button.style.marginLeft = -0.8f;
        yield return null;

        // Frame 2: sobrecompensación
        _button.style.marginTop = 0.3f;
        _button.style.marginLeft = 0.8f;
        yield return null;

        // Frame 3: asentamiento
        _button.style.marginTop = 0f;
        _button.style.marginLeft = 0f;
    }

    /// <summary>
    /// Simula blur reduciendo la opacidad del texto al inicio del hover y luego subiéndola.
    /// Como un CRT que ajusta el foco.
    /// </summary>
    IEnumerator SimulateBlur()
    {
        float elapsed = 0f;
        float phase1Duration = 0.06f;
        float phase2Duration = 0.14f;

        // Phase 1: opacidad baja a 0.80 (desenfoque)
        while (elapsed < phase1Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / phase1Duration;
            _button.style.opacity = Mathf.Lerp(1.0f, 0.80f, t);
            yield return null;
        }

        elapsed = 0f;

        // Phase 2: opacidad sube a 1.0 (enfoque perfecto)
        while (elapsed < phase2Duration)
        {
            elapsed += Time.unscaledDeltaTime;
            float t = elapsed / phase2Duration;
            _button.style.opacity = Mathf.Lerp(0.80f, 1.0f, t);
            yield return null;
        }

        _button.style.opacity = 1.0f;
    }

    /// <summary>
    /// Activa las ghost layers con la secuencia de delay analógico.
    /// </summary>
    IEnumerator ActivateGhostLayers()
    {
        // Ghost A — delay 15ms
        yield return new WaitForSecondsRealtime(0.015f);
        _ghostA.AddToClassList("nav-item-ghost-a--visible");
        _ghostA.style.color = new Color(0.788f, 0.831f, 0.690f, _system.GhostLayerAOpacity);

        // Ghost B — delay 30ms
        yield return new WaitForSecondsRealtime(0.015f);
        _ghostB.AddToClassList("nav-item-ghost-b--visible");
        _ghostB.style.color = new Color(0.788f, 0.831f, 0.690f, _system.GhostLayerBOpacity);
    }

    /// <summary>
    /// Activa la aberración cromática con delay de 10ms.
    /// </summary>
    IEnumerator ActivateChromaLayers()
    {
        yield return new WaitForSecondsRealtime(0.010f);
        _chromaR.AddToClassList("nav-item-chroma-r--visible");
        _chromaB.AddToClassList("nav-item-chroma-b--visible");
    }

    /// <summary>
    /// Activa el overlay de ruido con delay de 50ms.
    /// </summary>
    IEnumerator ActivateNoise()
    {
        yield return new WaitForSecondsRealtime(0.050f);
        _noiseOverlay.AddToClassList("nav-item-noise--visible");
        _noiseOverlay.style.opacity = _system.NoiseBaseOpacity;
    }

    IEnumerator FadeNoiseOut()
    {
        float start   = _noiseOverlay.style.opacity.value;
        float elapsed = 0f;
        float duration = 0.15f;

        while (elapsed < duration)
        {
            elapsed += Time.unscaledDeltaTime;
            _noiseOverlay.style.opacity = Mathf.Lerp(start, 0f, elapsed / duration);
            yield return null;
        }
        _noiseOverlay.style.opacity = 0f;
    }

    /// <summary>
    /// Spawnea 3–5 partículas de polvo de memoria al entrar en hover.
    /// </summary>
    IEnumerator SpawnHoverParticles()
    {
        int count = Random.Range(3, 6);

        for (int i = 0; i < count; i++)
        {
            _system.RunCoroutine(SpawnSingleParticle());
            yield return new WaitForSecondsRealtime(Random.Range(0.02f, 0.08f));
        }
    }

    IEnumerator SpawnSingleParticle()
    {
        // Crear partícula
        var particle = new VisualElement();
        bool isAmber = Random.value > 0.7f;
        particle.AddToClassList("ui-particle");
        particle.AddToClassList(isAmber ? "ui-particle--amber" : "ui-particle--sage");

        // Posición inicial: borde izquierdo del botón, Y aleatorio
        float startX = -2f;
        float startY = Random.Range(0.2f, 0.8f) * (_button.resolvedStyle.height > 0 ? _button.resolvedStyle.height : 36f);
        particle.style.left = startX;
        particle.style.top  = startY;
        particle.style.opacity = 0.8f;

        _button.Add(particle);

        // Animación de movimiento
        float speed    = Random.Range(12f, 28f) * _system.ParticleSpeed;
        float lifetime = Random.Range(0.4f, 0.8f);
        float angle    = Random.Range(150f, 210f) * Mathf.Deg2Rad; // Hacia la izquierda
        float vx       = Mathf.Cos(angle) * speed;
        float vy       = Mathf.Sin(angle) * speed;
        float elapsed  = 0f;

        while (elapsed < lifetime)
        {
            elapsed += Time.unscaledDeltaTime;
            startX += vx * Time.unscaledDeltaTime;
            startY += vy * Time.unscaledDeltaTime;
            particle.style.left = startX;
            particle.style.top  = startY;

            // Fade out en los últimos 200ms
            float fadeStart = lifetime - 0.2f;
            if (elapsed > fadeStart)
                particle.style.opacity = Mathf.Lerp(0.8f, 0f, (elapsed - fadeStart) / 0.2f);

            yield return null;
        }

        // Remover partícula del DOM
        _button.Remove(particle);
    }

    /// <summary>
    /// Secuencia completa de press: escala hacia abajo + flash ámbar + chroma intenso + recuperación.
    /// </summary>
    IEnumerator PressSequence()
    {
        // 1. Flash de confirmación ámbar
        _confirmFlash.AddToClassList("nav-item-confirm-flash--on");

        // 2. Aberración cromática intensificada
        _chromaR.RemoveFromClassList("nav-item-chroma-r--visible");
        _chromaR.AddToClassList("nav-item-chroma-r--pressed");
        _chromaB.RemoveFromClassList("nav-item-chroma-b--visible");
        _chromaB.AddToClassList("nav-item-chroma-b--pressed");

        // 3. USS maneja la escala vía :active, aquí esperamos
        yield return new WaitForSecondsRealtime(0.08f);

        // 4. Revertir efectos
        _confirmFlash.RemoveFromClassList("nav-item-confirm-flash--on");
        _chromaR.RemoveFromClassList("nav-item-chroma-r--pressed");
        _chromaR.AddToClassList("nav-item-chroma-r--visible");
        _chromaB.RemoveFromClassList("nav-item-chroma-b--pressed");
        _chromaB.AddToClassList("nav-item-chroma-b--visible");

        yield return new WaitForSecondsRealtime(0.12f);
        _currentState = State.HoverHeld;
    }

    IEnumerator DeactivateWithDelay(VisualElement el, string cssClass, float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        el.RemoveFromClassList(cssClass);
    }

    IEnumerator SetIdleAfterDelay(float delay)
    {
        yield return new WaitForSecondsRealtime(delay);
        _currentState = State.Idle;
    }

    // ═══════════════════════════════════════════════════════════════
    // LIMPIEZA
    // ═══════════════════════════════════════════════════════════════

    void StopAllLoops()
    {
        _system.StopRunningCoroutine(_hoverHeldLoop);
        _system.StopRunningCoroutine(_noiseLoop);
        _system.StopRunningCoroutine(_analogJitterLoop);
        _system.StopRunningCoroutine(_scanLineLoop);
        _hoverHeldLoop     = null;
        _noiseLoop         = null;
        _analogJitterLoop  = null;
        _scanLineLoop      = null;
    }

    /// <summary>
    /// Limpieza completa: remueve todos los VisualElements creados dinámicamente.
    /// Llamado desde MenuHoverSystem.OnDisable().
    /// </summary>
    public void Cleanup()
    {
        StopAllLoops();

        SafeRemove(_button, _chromaR);
        SafeRemove(_button, _chromaB);
        SafeRemove(_button, _noiseOverlay);
        SafeRemove(_button, _echoLayer);
        SafeRemove(_button, _focusPip);
        SafeRemove(_button, _confirmFlash);
        SafeRemove(_button, _scanLine);

        var parent = _button.parent;
        if (parent != null)
        {
            SafeRemove(parent, _ghostA);
            SafeRemove(parent, _ghostB);
        }
    }

    static void SafeRemove(VisualElement parent, VisualElement child)
    {
        if (parent != null && child != null && child.parent == parent)
            parent.Remove(child);
    }
}

/// <summary>Estructura para rastrear partículas activas (reserved for future pooling).</summary>
public struct UIParticleInstance
{
    public VisualElement Element;
    public float Lifetime;
    public float Elapsed;
}
