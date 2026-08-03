---
name: Echoes of You
colors:
  surface: '#131410'
  surface-dim: '#131410'
  surface-bright: '#393935'
  surface-container-lowest: '#0e0f0b'
  surface-container-low: '#1b1c18'
  surface-container: '#1f201c'
  surface-container-high: '#2a2a26'
  surface-container-highest: '#353530'
  on-surface: '#e4e2dc'
  on-surface-variant: '#ccc6bc'
  inverse-surface: '#e4e2dc'
  inverse-on-surface: '#30312c'
  outline: '#959087'
  outline-variant: '#4a463f'
  surface-tint: '#ccc6b9'
  primary: '#fffeff'
  on-primary: '#333027'
  primary-container: '#e8e1d4'
  on-primary-container: '#686459'
  inverse-primary: '#625e54'
  secondary: '#bfc7d7'
  on-secondary: '#29313d'
  secondary-container: '#444c59'
  on-secondary-container: '#b4bccc'
  tertiary: '#fffeff'
  on-tertiary: '#003543'
  tertiary-container: '#b6eaff'
  on-tertiary-container: '#006d87'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e9e2d5'
  primary-fixed-dim: '#ccc6b9'
  on-primary-fixed: '#1e1b14'
  on-primary-fixed-variant: '#4a463d'
  secondary-fixed: '#dbe3f3'
  secondary-fixed-dim: '#bfc7d7'
  on-secondary-fixed: '#141c28'
  on-secondary-fixed-variant: '#3f4754'
  tertiary-fixed: '#b7eaff'
  tertiary-fixed-dim: '#63d4fa'
  on-tertiary-fixed: '#001f28'
  on-tertiary-fixed-variant: '#004d61'
  background: '#131410'
  on-background: '#e4e2dc'
  surface-variant: '#353530'
typography:
  display-lg:
    fontFamily: ebGaramond
    fontSize: 64px
    fontWeight: '400'
    lineHeight: 72px
    letterSpacing: -0.02em
  display-md:
    fontFamily: ebGaramond
    fontSize: 48px
    fontWeight: '400'
    lineHeight: 56px
  interface-lg:
    fontFamily: courierPrime
    fontSize: 38px
    fontWeight: '400'
    lineHeight: 44px
  interface-md:
    fontFamily: courierPrime
    fontSize: 24px
    fontWeight: '400'
    lineHeight: 32px
  interface-sm:
    fontFamily: courierPrime
    fontSize: 20px
    fontWeight: '400'
    lineHeight: 28px
  label-caps:
    fontFamily: courierPrime
    fontSize: 16px
    fontWeight: '700'
    lineHeight: 20px
    letterSpacing: 0.1em
  display-lg-mobile:
    fontFamily: ebGaramond
    fontSize: 40px
    fontWeight: '400'
    lineHeight: 44px
spacing:
  unit: 8px
  gutter: 24px
  margin-edge: 64px
  container-padding: 32px
---

## Brand & Style
The design system reflects a melancholic exploration of corrupted memory, blending institutional school archives with the tactile decay of analog media. The UI serves as a "damaged notebook" or a "corrupted memory file," evoking an emotional response that is intimate, haunted, and scholarly.

The aesthetic fuses **Minimalism** with **Retro/Analog** influences. High-quality literary typography meets the technical rigidity of monospaced fonts. Surfaces are not solid; they are translucent layers representing layers of the subconscious. Visual motifs include a "Broken O" emblem, representing fragmentation, and "Echo Trails"—delayed silhouettes that suggest a presence just out of reach.

## Colors
This system utilizes a high-contrast, dark-mode foundation to simulate deep shadows and forgotten corridors.

- **Background (#08090B):** Total void. Used for the base canvas.
- **Corridor Navy (#1C2430):** Used for container backgrounds and deep UI layers.
- **Aged Ivory (#E8E1D4):** The primary text and structural stroke color. It suggests weathered paper and bone.
- **Echo Cyan (#4FC3E8):** Strictly reserved for temporal mechanics, glitch effects, and "Echo" systems. 
- **Memory Amber (#E8B262):** Used for critical narrative choices, saved data, and warmth in a cold environment.
- **Aiden Ribbon Red (#8F252D):** A deep, emotional accent color for specific character ties or tragic markers.
- **Secondary Gray (#9B9A94):** Low-priority metadata and disabled states.

## Typography
The typography contrasts the "Literary/Human" (EB Garamond) with the "System/Archive" (Courier Prime). 

Titles and chapter headings use **EB Garamond** to evoke a sense of history and literature. All functional UI, labels, and terminal-style readouts use **Courier Prime**. 

For all monospaced interface text, avoid kerning adjustments. Maintain the rigid grid of the typewriter. Use `label-caps` for secondary metadata or categorizing archive files.

## Layout & Spacing
The layout follows a 1920x1080 canvas standard but utilizes a flexible **No Grid** contextual philosophy for narrative elements to simulate a "scattered" notebook feel. 

- **Safe Margins:** Keep primary UI elements 64px from the screen edge.
- **The Archive Column:** Menus should be left-aligned or center-aligned, avoiding right-side weight unless representing "Temporal" data.
- **Ultrawide Handling:** Content remains centered in a 16:9 container, while "Echo" silhouettes and glitch particles are permitted to bleed into the ultrawide wings.

## Elevation & Depth
Depth is created through **Glassmorphism** and **Tonal Layering** rather than traditional shadows.

1.  **Backdrop Blurs:** Use subtle background blurs (8px to 16px) on all translucent Navy containers (62-78% opacity).
2.  **Ghost Outlines:** Elements are defined by thin 1px borders in Aged Ivory or Echo Cyan. No heavy drop shadows are permitted; use "delayed silhouettes" (a 10% opacity duplicate of the element offset by 4px) to suggest depth in time rather than space.
3.  **Corruption Layers:** Apply a subtle scanline or noise texture overlay to the highest elevation elements to signify "active" memory.

## Shapes
The shape language is strictly **Sharp (0px)**. Rounded corners are non-existent in this archive. 

Rectangles should feel like index cards, polaroids, or terminal windows. The only exception to the "sharp" rule is the **Broken O** motif, which should be used as a decorative frame or a loading indicator, rendered with a fragmented, hand-drawn stroke quality.

## Components
### Buttons & Navigation
- **Default State:** Transparent background, 1px Aged Ivory border, Courier Prime text.
- **Hover State (PS2 Style):** Background fills with Corridor Navy. Trigger a "chromatic offset" effect where the text splits into faint Cyan and Red channels. Apply a micro-jitter (1px random translation).
- **Active State:** Border shifts to Memory Amber.

### Cards & Containers
- Use 70% opacity Corridor Navy backgrounds with a 1px ivory border. 
- Headers should be separated by a 1px horizontal rule.

### Input Fields & Terminal
- Represented as a simple underscore `_` cursor. 
- Text entry should have a "frame-skipping" delay, where characters appear 2-3 frames after the keystroke to simulate a struggling processor.

### Echo Systems (Temporal UI)
- Any component related to "Echos" must use the Echo Cyan color.
- These components should have a persistent "delayed duplicate" silhouette trailing slightly behind the main element during movement.

### Chips & Tags
- Used for "Memory Fragments." Rectangular boxes with Courier Prime text. If a fragment is "Corrupted," use Aiden Ribbon Red for the border and strike through the text.