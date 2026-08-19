namespace Echoes.Interaction
{
    /// <summary>
    /// Categoría contextual de un objeto interactuable.
    /// Determina cómo el InteractionSystem trata al objeto: qué prompt muestra,
    /// qué feedback reproduce y qué acción ejecuta al pulsar E.
    ///   A) Gameplay  — avanza el nivel / resuelve puzzles (puertas, palancas, pistas).
    ///   B) Narrative — revela información (inspección / diálogo VN).
    ///   C) Ambient   — reacciona sin abrir interfaz (sonido + animación sutil).
    ///   D) Decoration — NO es interactuable; nunca lleva este componente.
    /// </summary>
    public enum InteractableCategory
    {
        Gameplay = 0,
        Narrative = 1,
        Ambient = 2
    }
}