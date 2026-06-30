import os
import uuid

# Ruta del directorio de los blueprints en el proyecto
BLUEPRINT_DIR = r"c:\Users\lol xdd\OneDrive\Documentos\Colegio\Echoes of you\Assets\Data\Levels"

# Asegurar que el directorio exista
os.makedirs(BLUEPRINT_DIR, exist_ok=True)

# GUID del script LevelBlueprint en este proyecto
SCRIPT_GUID = "5be9d0c6961d9b24992d8c11589b2350"

def generate_unity_guid():
    return uuid.uuid4().hex

def create_yaml_asset(level_idx, config):
    level_name = f"Level_{level_idx:02d}"
    next_level = f"Level_{level_idx+1:02d}" if level_idx < 15 else "MainMenu"
    
    # Header YAML
    lines = [
        "%YAML 1.1",
        "%TAG !u! tag:unity3d.com,2011:",
        "--- !u!114 &11400000",
        "MonoBehaviour:",
        "  m_ObjectHideFlags: 0",
        "  m_CorrespondingSourceObject: {fileID: 0}",
        "  m_PrefabInstance: {fileID: 0}",
        "  m_PrefabAsset: {fileID: 0}",
        "  m_GameObject: {fileID: 0}",
        "  m_Enabled: 1",
        "  m_EditorHideFlags: 0",
        f"  m_Script: {{fileID: 11500000, guid: {SCRIPT_GUID}, type: 3}}",
        f"  m_Name: {level_name}_Blueprint",
        "  m_EditorClassIdentifier: Assembly-CSharp::LevelBlueprint",
        f"  levelName: {level_name}",
        f"  nextLevel: {next_level}",
        f"  actNumber: {config['act']}",
        f"  archetype: {config['archetype']}",
        f"  maxEchoes: {config['max_echoes']}",
        f"  maxRecordSeconds: {config['max_record_seconds']}",
        "  activeParadoxes: ",
        f"  fogColor: {{r: {config['fog_color'][0]}, g: {config['fog_color'][1]}, b: {config['fog_color'][2]}, a: 1}}",
        f"  fogDensity: {config['fog_density']}",
        f"  skyColor: {{r: {config['sky_color'][0]}, g: {config['sky_color'][1]}, b: {config['sky_color'][2]}, a: 1}}",
        f"  directionalLightRotation: {{x: {config['dir_light_rot'][0]}, y: {config['dir_light_rot'][1]}, z: {config['dir_light_rot'][2]}}}",
        f"  directionalLightColor: {{r: {config['dir_light_col'][0]}, g: {config['dir_light_col'][1]}, b: {config['dir_light_col'][2]}, a: 1}}",
        f"  directionalLightIntensity: {config['dir_light_intensity']}",
        f"  narrativeIntroTitle: \"Nivel {level_idx} \\u2014 {config['title']}\"",
        f"  narrativeIntroDesc: \"{config['intro_desc']}\"",
        f"  narrativeIntroDuration: {config['intro_duration']}",
        f"  puzzleObjectiveText: {config['objective']}",
        f"  puzzleActiveText: \"{config['puzzle_active']}\"",
        f"  puzzleCompleteText: \"{config['puzzle_complete']}\"",
        "  pathHints:"
    ]
    
    # Path Hints
    for hint in config['path_hints']:
        lines.append(f"  - {{x: {hint[0]}, y: {hint[1]}, z: {hint[2]}}}")
        
    lines.append("  modules:")
    
    # Modules list
    for mod in config['modules']:
        lines.extend([
            f"  - name: {mod['name']}",
            f"    type: {mod['type']}",
            f"    position: {{x: {mod['pos'][0]}, y: {mod['pos'][1]}, z: {mod['pos'][2]}}}",
            f"    rotation: {{x: {mod['rot'][0]}, y: {mod['rot'][1]}, z: {mod['rot'][2]}}}",
            f"    scale: {{x: {mod['scale'][0]}, y: {mod['scale'][1]}, z: {mod['scale'][2]}}}",
            f"    customData: {mod['custom_data'] if mod['custom_data'] is not None else ''}",
            "    targetSignals:"
        ])
        for sig in mod['target_signals']:
            lines.append(f"    - {sig}")
            
    return "\n".join(lines) + "\n"

def create_yaml_meta(guid):
    return f"""fileFormatVersion: 2
guid: {guid}
NativeFormatImporter:
  externalObjects: {{}}
  mainObjectFileID: 11400000
  userData: 
  assetBundleName: 
  assetBundleVariant: 
"""

# Definición de configuraciones de los 15 niveles
level_configs = {
    1: {
        "act": 1, "archetype": 0, "max_echoes": 0, "max_record_seconds": 0,
        "title": "Desorientación",
        "intro_desc": "Despiertas en un pasillo interminable. Camina a través de la bruma escolar. Encuentra la salida de tus propios pensamientos.",
        "intro_duration": 8,
        "objective": "Camina a través del pasillo liminal hacia la luz de la meta.",
        "puzzle_active": "La bruma del colegio te rodea.",
        "puzzle_complete": "Camino inicial despejado.",
        "fog_color": (0.15, 0.15, 0.15), "fog_density": 0.035, "sky_color": (0.1, 0.1, 0.1),
        "dir_light_rot": (45, -45, 0), "dir_light_col": (0.8, 0.85, 0.9), "dir_light_intensity": 0.6,
        "path_hints": [(0, 1.1, -10), (0, 1.1, 13), (15, 1.1, 13)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -10), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "Corridor1", "type": 24, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (6, 0.5, 20), "custom_data": None, "target_signals": []},
            {"name": "Corner1", "type": 28, "pos": (0, 0, 13), "rot": (0, 0, 0), "scale": (6, 0.5, 6), "custom_data": None, "target_signals": []},
            {"name": "Corridor2", "type": 24, "pos": (8, 0, 13), "rot": (0, 90, 0), "scale": (6, 0.5, 16), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (15, 1.1, 13), "rot": (0, 90, 0), "scale": (1, 1, 1), "custom_data": "Level_02", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (15, 1.1, 13), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Camina hacia la meta a través del pasillo escolar.|Bruma densa.|Salida alcanzada.", "target_signals": []},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Encuentra la salida.|La bruma te rodea.|Saliste del pasillo.", "target_signals": []}
        ]
    },
    2: {
        "act": 1, "archetype": 0, "max_echoes": 1, "max_record_seconds": 8,
        "title": "Repetición",
        "intro_desc": "El tiempo se repite en el aula de clase. Graba un eco presionando el pedal en el aula anterior para abrir la puerta de la siguiente.",
        "intro_duration": 8,
        "objective": "Usa tu eco para abrir la compuerta intermedia del aula.",
        "puzzle_active": "Sincroniza el pedal del aula.",
        "puzzle_complete": "Compuerta del aula superada.",
        "fog_color": (0.12, 0.14, 0.18), "fog_density": 0.025, "sky_color": (0.15, 0.17, 0.22),
        "dir_light_rot": (50, -30, 0), "dir_light_col": (0.85, 0.9, 0.95), "dir_light_intensity": 0.8,
        "path_hints": [(0, 1.1, -12), (0, 1.1, 10)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "Aula1", "type": 21, "pos": (0, 0, -6), "rot": (0, 0, 0), "scale": (10, 0.5, 10), "custom_data": None, "target_signals": []},
            {"name": "Plate1", "type": 4, "pos": (-3, 0.33, -6), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "Door1", "type": 5, "pos": (0, 1.5, 0), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["Plate1"]},
            {"name": "Aula2", "type": 21, "pos": (0, 0, 6), "rot": (0, 0, 0), "scale": (10, 0.5, 10), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 10), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_03", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 10), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Usa tu eco en el pedal para abrir el portón.|El eco abre la compuerta.|Aula superada.", "target_signals": ["Plate1"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Proyecta un eco en el pedal.|Tu pasado abre el camino.|Camino libre.", "target_signals": []},
            {"name": "Tut_L02", "type": 9, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (8, 3, 4), "custom_data": "Nivel 2 — Repetición|Graba un eco presionando la alfombrilla del Aula 1. Luego cruza el portón del centro mientras el eco la pisa.", "target_signals": []}
        ]
    },
    3: {
        "act": 1, "archetype": 0, "max_echoes": 1, "max_record_seconds": 10,
        "title": "Indecisión",
        "intro_desc": "El vestíbulo escolar se divide. Un camino hacia la izquierda abre la compuerta de la derecha. Envía tu eco a decidir por ti.",
        "intro_duration": 8,
        "objective": "Envía tu eco por el pasillo izquierdo para abrir la reja del pasillo derecho.",
        "puzzle_active": "Placa izquierda pulsada.",
        "puzzle_complete": "Salida derecha franqueada.",
        "fog_color": (0.1, 0.12, 0.15), "fog_density": 0.02, "sky_color": (0.12, 0.14, 0.18),
        "dir_light_rot": (55, -45, 0), "dir_light_col": (0.8, 0.8, 0.85), "dir_light_intensity": 0.7,
        "path_hints": [(0, 1.1, -12), (-6, 1.1, 4), (6, 1.1, 9)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "VestibuloBase", "type": 0, "pos": (0, 0, -8), "rot": (0, 0, 0), "scale": (8, 0.5, 6), "custom_data": None, "target_signals": []},
            {"name": "CorridorL", "type": 24, "pos": (-6, 0, 0), "rot": (0, 0, 0), "scale": (4, 0.5, 14), "custom_data": None, "target_signals": []},
            {"name": "CorridorR", "type": 24, "pos": (6, 0, 0), "rot": (0, 0, 0), "scale": (4, 0.5, 14), "custom_data": None, "target_signals": []},
            {"name": "PlateL", "type": 4, "pos": (-6, 0.33, 4), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "DoorR", "type": 5, "pos": (6, 1.5, 4), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["PlateL"]},
            {"name": "LevelExit_Area", "type": 6, "pos": (6, 1.1, 9), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_04", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (6, 1.1, 9), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Envía tu eco a la placa izquierda.|El pasillo derecho se abre.|Vestíbulo superado.", "target_signals": ["PlateL"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Usa tu eco como bifurcador.|La reja del pasillo derecho responde.|Camino libre.", "target_signals": []}
        ]
    },
    4: {
        "act": 1, "archetype": 0, "max_echoes": 1, "max_record_seconds": 15,
        "title": "Espera",
        "intro_desc": "La oficina de archivos requiere paciencia. Mantén presionado el interruptor con tu eco durante 10 segundos enteros mientras Aiden cruza el túnel.",
        "intro_duration": 8,
        "objective": "Graba un eco parado en la placa de la dirección para abrir la compuerta del sótano.",
        "puzzle_active": "Eco esperando en la oficina.",
        "puzzle_complete": "Acceso a los túneles abierto.",
        "fog_color": (0.08, 0.08, 0.12), "fog_density": 0.03, "sky_color": (0.1, 0.1, 0.15),
        "dir_light_rot": (40, -60, 0), "dir_light_col": (0.9, 0.75, 0.5), "dir_light_intensity": 0.5,
        "path_hints": [(0, 1.1, -10), (-5, 1.1, 0), (5, -1.9, 14)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -10), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "Oficina", "type": 27, "pos": (-5, 0, 0), "rot": (0, 0, 0), "scale": (8, 0.5, 8), "custom_data": None, "target_signals": []},
            {"name": "OfficePlate", "type": 4, "pos": (-5, 0.33, 0), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "TunelSotano", "type": 24, "pos": (5, -3, 4), "rot": (0, 0, 0), "scale": (4, 0.5, 16), "custom_data": None, "target_signals": []},
            {"name": "DoorSotano", "type": 5, "pos": (5, -1.5, 10), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["OfficePlate"]},
            {"name": "LevelExit_Area", "type": 6, "pos": (5, -1.9, 14), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_05", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (5, -1.9, 14), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Graba un eco quieto en la oficina por 10 segundos.|El sótano permanece abierto.|Sótano superado.", "target_signals": ["OfficePlate"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Para el eco en la oficina.|Espera a que el temporizador corra.|Paso libre.", "target_signals": []}
        ]
    },
    5: {
        "act": 1, "archetype": 0, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Culpa",
        "intro_desc": "El laboratorio escolar está electrificado. El vapor caliente daña al eco. Memoriza el patrón para grabar una ruta evasiva perfecta.",
        "intro_duration": 8,
        "objective": "Esquiva el campo de vapor con tu eco para desactivar la esclusa del fondo.",
        "puzzle_active": "Eco avanzando en la trampa.",
        "puzzle_complete": "Laboratorio neutralizado.",
        "fog_color": (0.16, 0.08, 0.08), "fog_density": 0.028, "sky_color": (0.2, 0.1, 0.1),
        "dir_light_rot": (45, -30, 0), "dir_light_col": (1.0, 0.5, 0.3), "dir_light_intensity": 0.6,
        "path_hints": [(0, 1.1, -12), (0, 1.1, 4), (0, 1.1, 12)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "LabGrande", "type": 21, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (12, 0.5, 16), "custom_data": None, "target_signals": []},
            {"name": "VaporToxico", "type": 17, "pos": (0, 0.5, 0), "rot": (0, 0, 0), "scale": (10, 3, 2), "custom_data": None, "target_signals": []},
            {"name": "PlateLab", "type": 4, "pos": (0, 0.33, 4), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "DoorLab", "type": 5, "pos": (0, 1.5, 8), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["PlateLab"]},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 12), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_06", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Esquiva el vapor y pisa el pedal.|El eco desactiva la compuerta.|Camino liberado.", "target_signals": ["PlateLab"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Evita el vapor con tu eco.|El eco mantiene el paso abierto.|Frontera superada.", "target_signals": []}
        ]
    },
    6: {
        "act": 2, "archetype": 1, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Negación",
        "intro_desc": "La biblioteca de la memoria es inestable. Las estanterías cambian de posición. Graba un eco que mire el estante para congelarlo en su lugar.",
        "intro_duration": 8,
        "objective": "Usa tu eco para fijar la estantería móvil y cruzar la biblioteca.",
        "puzzle_active": "Estantería fijada por el eco.",
        "puzzle_complete": "Biblioteca estabilizada.",
        "fog_color": (0.05, 0.08, 0.12), "fog_density": 0.03, "sky_color": (0.08, 0.1, 0.15),
        "dir_light_rot": (60, -45, 0), "dir_light_col": (0.6, 0.7, 0.9), "dir_light_intensity": 0.5,
        "path_hints": [(0, 1.1, -12), (-4, 1.1, 0), (0, 1.1, 12)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "BibliotecaStack", "type": 30, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (12, 0.5, 20), "custom_data": None, "target_signals": []},
            {"name": "AnclaMirada", "type": 23, "pos": (-4, 0, 0), "rot": (0, 0, 0), "scale": (4, 0.5, 4), "custom_data": None, "target_signals": []},
            {"name": "EstanteriaMovel", "type": 8, "pos": (0, 0.25, 4), "rot": (0, 0, 0), "scale": (6, 3, 2), "custom_data": "0,0,0|0,6,0|4", "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 12), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_07", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Usa tu eco para fijar la estantería móvil.|El eco bloquea el movimiento.|Biblioteca superada.", "target_signals": []},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Mide la sincronía del estante.|El estante está en posición.|Paso libre.", "target_signals": []}
        ]
    },
    7: {
        "act": 2, "archetype": 1, "max_echoes": 1, "max_record_seconds": 14,
        "title": "Evasión",
        "intro_desc": "Las duchas y vestuarios están custodiados por sombras de culpa. Graba un eco que corra ruidosamente hacia el rincón para distraerlas.",
        "intro_duration": 8,
        "objective": "Desvía la atención de las sombras de niebla usando el eco como señuelo.",
        "puzzle_active": "Eco atrayendo a las sombras.",
        "puzzle_complete": "Sombras de la memoria burladas.",
        "fog_color": (0.08, 0.12, 0.1), "fog_density": 0.026, "sky_color": (0.1, 0.15, 0.12),
        "dir_light_rot": (40, -30, 0), "dir_light_col": (0.5, 0.8, 0.6), "dir_light_intensity": 0.4,
        "path_hints": [(0, 1.1, -15), (-4, 1.1, -2), (0, 1.1, 14)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -15), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "DuchasGym", "type": 26, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (12, 0.5, 24), "custom_data": None, "target_signals": []},
            {"name": "TrampaSombra", "type": 18, "pos": (0, 0.5, 2), "rot": (0, 0, 0), "scale": (10, 3, 8), "custom_data": None, "target_signals": []},
            {"name": "PlateRuido", "type": 4, "pos": (-4, 0.33, -2), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 14), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_08", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 14), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Usa tu eco como señuelo para las sombras.|Las sombras siguen el ruido.|Vestuarios superados.", "target_signals": []},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Crea ruido con tu eco.|Las sombras persiguen el eco.|Camino despejado.", "target_signals": []}
        ]
    },
    8: {
        "act": 2, "archetype": 1, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Autosabotaje",
        "intro_desc": "El hueco de la escalera principal está destruido. No pises la misma plataforma que tu eco; colapsará. Coordina tus pasos con su pasado.",
        "intro_duration": 8,
        "objective": "Sube por las escaleras flotantes coordinándote temporalmente con tu eco.",
        "puzzle_active": "Eco avanzando en las escaleras.",
        "puzzle_complete": "Escalera colapsada superada.",
        "fog_color": (0.05, 0.05, 0.05), "fog_density": 0.035, "sky_color": (0.02, 0.02, 0.02),
        "dir_light_rot": (65, 0, 0), "dir_light_col": (0.9, 0.9, 0.95), "dir_light_intensity": 0.6,
        "path_hints": [(0, 1.1, -12), (0, 3.1, 0), (0, 6.6, 14)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "TorreEscalera", "type": 29, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (10, 0.5, 10), "custom_data": None, "target_signals": []},
            {"name": "PuentePupitres", "type": 22, "pos": (0, 3, 8), "rot": (0, 0, 0), "scale": (4, 0.5, 12), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 6.6, 14), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_09", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 6.6, 14), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Coordinación estricta de saltos con tu eco.|El puente responde al eco.|Escaleras superadas.", "target_signals": []},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Sigue el compás del eco.|El eco despeja el paso.|Paso superior desbloqueado.", "target_signals": []}
        ]
    },
    9: {
        "act": 3, "archetype": 2, "max_echoes": 2, "max_record_seconds": 16,
        "title": "Control",
        "intro_desc": "El aula de música tiene dos ecos activos. Sincroniza a Echo_A en la cabina 1 y a Echo_B en la cabina 2 para cruzar las rejas secuenciales.",
        "intro_duration": 8,
        "objective": "Sincroniza dos ecos en las válvulas de las cabinas para abrir las dos rejas de salida.",
        "puzzle_active": "Ambos ecos activos en las cabinas.",
        "puzzle_complete": "Cabinas de música sincronizadas.",
        "fog_color": (0.1, 0.08, 0.14), "fog_density": 0.024, "sky_color": (0.12, 0.1, 0.18),
        "dir_light_rot": (45, -45, 0), "dir_light_col": (0.9, 0.7, 0.85), "dir_light_intensity": 0.7,
        "path_hints": [(0, 1.1, -12), (-5, 1.1, 0), (5, 1.1, 0), (0, 1.1, 16)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "CabinaA", "type": 21, "pos": (-5, 0, 0), "rot": (0, 0, 0), "scale": (6, 0.5, 6), "custom_data": None, "target_signals": []},
            {"name": "CabinaB", "type": 21, "pos": (5, 0, 0), "rot": (0, 0, 0), "scale": (6, 0.5, 6), "custom_data": None, "target_signals": []},
            {"name": "PlateMusicA", "type": 4, "pos": (-5, 0.33, 0), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "PlateMusicB", "type": 4, "pos": (5, 0.33, 0), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "DoorMusicA", "type": 5, "pos": (0, 1.5, 6), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["PlateMusicA"]},
            {"name": "DoorMusicB", "type": 5, "pos": (0, 1.5, 12), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["PlateMusicB"]},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 16), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_10", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 16), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Sincroniza dos ecos en las placas A y B.|Las puertas se abren en orden.|Aula de música superada.", "target_signals": ["PlateMusicA", "PlateMusicB"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Graba dos ecos distintos.|Los ecos tocan los acordes pasados.|Camino libre.", "target_signals": []}
        ]
    },
    10: {
        "act": 3, "archetype": 2, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Recuerdos",
        "intro_desc": "El pasillo de arte revela dibujos del pasado. Tu eco parado frente al cuadro revelará el código para abrir la puerta de salida.",
        "intro_duration": 8,
        "objective": "Usa tu eco frente al cuadro de arte para revelar el pasadizo.",
        "puzzle_active": "Eco interactuando con el cuadro.",
        "puzzle_complete": "Pasadizo de arte abierto.",
        "fog_color": (0.12, 0.12, 0.12), "fog_density": 0.02, "sky_color": (0.15, 0.15, 0.15),
        "dir_light_rot": (45, -30, 0), "dir_light_col": (1.0, 0.8, 0.6), "dir_light_intensity": 0.8,
        "path_hints": [(0, 1.1, -12), (0, 1.1, 0), (0, 1.1, 14)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "PasilloArte", "type": 24, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (6, 0.5, 20), "custom_data": None, "target_signals": []},
            {"name": "CuadroArte", "type": 23, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (4, 0.5, 4), "custom_data": None, "target_signals": []},
            {"name": "DoorArte", "type": 5, "pos": (0, 1.5, 8), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["CuadroArte"]},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 14), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_11", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 14), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Coloca a tu eco frente al cuadro.|El cuadro revela el pasadizo.|Recuerdos superados.", "target_signals": ["CuadroArte"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Pisa el ancla con tu eco.|El recuerdo abre la puerta secreta.|Camino libre.", "target_signals": []}
        ]
    },
    11: {
        "act": 3, "archetype": 2, "max_echoes": 1, "max_record_seconds": 14,
        "title": "Conexión",
        "intro_desc": "El patio interior de la escuela está inundado por la lluvia. Contrapesa la gran plataforma basculante con la masa de tu eco.",
        "intro_duration": 8,
        "objective": "Coloca a tu eco en el extremo izquierdo de la plataforma basculante para poder subir por la derecha.",
        "puzzle_active": "Eco equilibrando la plataforma.",
        "puzzle_complete": "Plataforma basculante superada.",
        "fog_color": (0.1, 0.12, 0.16), "fog_density": 0.022, "sky_color": (0.12, 0.14, 0.18),
        "dir_light_rot": (50, -45, 0), "dir_light_col": (0.7, 0.75, 0.8), "dir_light_intensity": 0.5,
        "path_hints": [(-8, 1.1, -12), (-5, 1.1, 2), (8, 4.5, 12)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (-8, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "PatioRecreo", "type": 25, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (24, 0.5, 24), "custom_data": None, "target_signals": []},
            {"name": "PlatBasculante", "type": 8, "pos": (0, 0, 2), "rot": (0, 0, 0), "scale": (12, 0.5, 4), "custom_data": "0,0,0|0,3,0|3", "target_signals": []},
            {"name": "PlatePatio", "type": 4, "pos": (-5, 0.33, 2), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (8, 4.5, 12), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_12", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (8, 4.5, 12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Usa tu eco en el contrapeso izquierdo.|La plataforma se eleva por la derecha.|Patio superado.", "target_signals": []},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Coloca el eco como contrapeso.|El eco sube el andamio.|Camino libre.", "target_signals": []}
        ]
    },
    12: {
        "act": 4, "archetype": 3, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Conflicto",
        "intro_desc": "El gimnasio escolar alberga recuerdos rotos. Tu propio eco se corrompe y te persigue. Evita cruzarte con tu trayectoria pasada.",
        "intro_duration": 8,
        "objective": "Esquiva a tu eco corrupto que persigue tus pasos para alcanzar la puerta final.",
        "puzzle_active": "Eco corrupto en persecución.",
        "puzzle_complete": "Gimnasio superado a salvo.",
        "fog_color": (0.14, 0.08, 0.12), "fog_density": 0.025, "sky_color": (0.16, 0.1, 0.14),
        "dir_light_rot": (45, -30, 0), "dir_light_col": (0.9, 0.6, 0.8), "dir_light_intensity": 0.5,
        "path_hints": [(0, 1.1, -16), (0, 1.1, 12), (0, 1.1, 18)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -16), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "Gimnasio", "type": 25, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (20, 0.5, 32), "custom_data": None, "target_signals": []},
            {"name": "TrapConflicto", "type": 18, "pos": (0, 0.5, 0), "rot": (0, 0, 0), "scale": (18, 3, 10), "custom_data": None, "target_signals": []},
            {"name": "PlateGym", "type": 4, "pos": (0, 0.33, 12), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "DoorGym", "type": 5, "pos": (0, 1.5, 15), "rot": (0, 0, 0), "scale": (3, 3, 0.5), "custom_data": None, "target_signals": ["PlateGym"]},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 18), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_13", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 18), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Pisa el pedal evadiendo tu propio eco.|El eco corrupto te persigue.|Gimnasio superado.", "target_signals": ["PlateGym"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Corre hacia el pedal.|Huye de tu propio eco pasado.|Puerta abierta.", "target_signals": []}
        ]
    },
    13: {
        "act": 4, "archetype": 3, "max_echoes": 1, "max_record_seconds": 15,
        "title": "Verdad",
        "intro_desc": "El aula de Lyra suspendida en el abismo. Coloca tres ecos en posiciones de recuerdo simultáneamente para reconstruir la verdad.",
        "intro_duration": 8,
        "objective": "Pisa los tres puntos del Aula de Lyra para desvelar el diálogo y abrir la esclusa de salida.",
        "puzzle_active": "Flashback de verdad activo.",
        "puzzle_complete": "La verdad ha sido aceptada.",
        "fog_color": (0.1, 0.1, 0.1), "fog_density": 0.03, "sky_color": (0.08, 0.08, 0.08),
        "dir_light_rot": (50, -30, 0), "dir_light_col": (1.0, 0.85, 0.65), "dir_light_intensity": 0.8,
        "path_hints": [(0, 1.1, -12), (0, 1.1, 10)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "AulaLyra", "type": 21, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (12, 0.5, 12), "custom_data": None, "target_signals": []},
            {"name": "PlateVerdad1", "type": 4, "pos": (-4, 0.33, 4), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "PlateVerdad2", "type": 4, "pos": (4, 0.33, 4), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "PlateVerdad3", "type": 4, "pos": (0, 0.33, 0), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 10), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_14", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 10), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Pisa las tres áreas del flashback escolar.|El diálogo se completa.|Verdad revelada.", "target_signals": ["PlateVerdad1", "PlateVerdad2", "PlateVerdad3"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Busca los puntos del recuerdo.|Reconstruye la escena del aula.|Salida abierta.", "target_signals": []}
        ]
    },
    14: {
        "act": 4, "archetype": 3, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Aceptación",
        "intro_desc": "El camino del no retorno. Tu eco camina sobre el abismo construyendo un puente de pupitres invisibles. Sigue su trayectoria exacta.",
        "intro_duration": 8,
        "objective": "Cruza el puente flotante de pupitres siguiendo la trayectoria de tu eco.",
        "puzzle_active": "Puente de pupitres materializado.",
        "puzzle_complete": "Fractura del abismo cruzada.",
        "fog_color": (0.2, 0.2, 0.25), "fog_density": 0.035, "sky_color": (0.22, 0.22, 0.28),
        "dir_light_rot": (45, -45, 0), "dir_light_col": (1.0, 1.0, 1.0), "dir_light_intensity": 0.9,
        "path_hints": [(0, 1.1, -14), (0, 1.1, 12)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (0, 1.1, -14), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "PlatInicio", "type": 0, "pos": (0, 0, -10), "rot": (0, 0, 0), "scale": (4, 0.5, 4), "custom_data": None, "target_signals": []},
            {"name": "PuentePupitresaccept", "type": 22, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (4, 0.5, 16), "custom_data": None, "target_signals": []},
            {"name": "PlatFinal", "type": 0, "pos": (0, 0, 10), "rot": (0, 0, 0), "scale": (4, 0.5, 4), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (0, 1.1, 12), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "Level_15", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (0, 1.1, 12), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Sigue los pasos de tu eco sobre la nada.|El eco materializa los pupitres.|Camino cruzado.", "target_signals": []},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Proyecta al eco sobre el vacío.|Camina sobre sus huellas del pasado.|Meta alcanzada.", "target_signals": []}
        ]
    },
    15: {
        "act": 4, "archetype": 3, "max_echoes": 1, "max_record_seconds": 12,
        "title": "Integración",
        "intro_desc": "La azotea de la escuela al amanecer. El eco y Aiden se mueven en simetría. Colisiónalos en el centro para fusionarse y ser libre.",
        "intro_duration": 8,
        "objective": "Guía al eco y a ti mismo para que colisionen en el centro de la azotea escolar al amanecer.",
        "puzzle_active": "Eco en movimiento simétrico.",
        "puzzle_complete": "Eco integrado. Has aceptado el pasado.",
        "fog_color": (0.24, 0.16, 0.18), "fog_density": 0.015, "sky_color": (0.3, 0.2, 0.22),
        "dir_light_rot": (20, -75, 0), "dir_light_col": (1.0, 0.65, 0.55), "dir_light_intensity": 1.0,
        "path_hints": [(-8, 1.1, 0), (0, 1.1, 0), (8, 1.1, 0)],
        "modules": [
            {"name": "PlayerStart", "type": 7, "pos": (-8, 1.1, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": None, "target_signals": []},
            {"name": "Azotea", "type": 25, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (24, 0.5, 24), "custom_data": None, "target_signals": []},
            {"name": "PlateCentro", "type": 4, "pos": (0, 0.33, 0), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": None, "target_signals": []},
            {"name": "LevelExit_Area", "type": 6, "pos": (8, 1.1, 0), "rot": (0, 0, 0), "scale": (1, 1, 1), "custom_data": "MainMenu", "target_signals": []},
            {"name": "LevelGoal", "type": 13, "pos": (8, 1.1, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Guía al eco para colisionar en el centro.|Fusión simétrica.|Integración completada.", "target_signals": ["PlateCentro"]},
            {"name": "LevelRuntime", "type": 14, "pos": (0, 0, 0), "rot": (0, 0, 0), "scale": (0, 0, 0), "custom_data": "Camina al centro simétricamente.|Fusiona el pasado con el presente.|Es hora de despertar.", "target_signals": []}
        ]
    }
}

# Generar todos los archivos .asset y sus .meta
for idx, config in level_configs.items():
    filename = f"Level_{idx:02d}_Blueprint.asset"
    filepath = os.path.join(BLUEPRINT_DIR, filename)
    meta_filepath = filepath + ".meta"
    
    # 1. Escribir Asset
    asset_content = create_yaml_asset(idx, config)
    with open(filepath, "w", encoding="utf-8") as f:
        f.write(asset_content)
    print(f"Created/Updated: {filepath}")
    
    # 2. Escribir Meta si no existe
    if not os.path.exists(meta_filepath):
        guid = generate_unity_guid()
        meta_content = create_yaml_meta(guid)
        with open(meta_filepath, "w", encoding="utf-8") as f:
            f.write(meta_content)
        print(f"Created Meta: {meta_filepath} with GUID: {guid}")

print("All 15 Level Blueprints created successfully.")
