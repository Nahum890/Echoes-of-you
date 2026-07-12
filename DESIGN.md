---
name: Echoes of You 2.0
colors:
  surface: '#131314'
  surface-dim: '#131314'
  surface-bright: '#3a393a'
  surface-container-lowest: '#0e0e0f'
  surface-container-low: '#1c1b1d'
  surface-container: '#201f21'
  surface-container-high: '#2a2a2b'
  surface-container-highest: '#353436'
  on-surface: '#e5e1e3'
  on-surface-variant: '#c7c5cd'
  inverse-surface: '#e5e1e3'
  inverse-on-surface: '#313031'
  outline: '#919097'
  outline-variant: '#46464c'
  surface-tint: '#c4c5db'
  primary: '#c4c5db'
  on-primary: '#2d2f40'
  primary-container: '#1a1c2c'
  on-primary-container: '#828498'
  inverse-primary: '#5c5d70'
  secondary: '#c9c99e'
  on-secondary: '#313213'
  secondary-container: '#484927'
  on-secondary-container: '#b7b88d'
  tertiary: '#f6be3d'
  on-tertiary: '#402d00'
  tertiary-container: '#281b00'
  on-tertiary-container: '#ab7e00'
  error: '#ffb4ab'
  on-error: '#690005'
  error-container: '#93000a'
  on-error-container: '#ffdad6'
  primary-fixed: '#e1e1f7'
  primary-fixed-dim: '#c4c5db'
  on-primary-fixed: '#181a2a'
  on-primary-fixed-variant: '#444657'
  secondary-fixed: '#e5e5b8'
  secondary-fixed-dim: '#c9c99e'
  on-secondary-fixed: '#1c1d02'
  on-secondary-fixed-variant: '#484927'
  tertiary-fixed: '#ffdea2'
  tertiary-fixed-dim: '#f6be3d'
  on-tertiary-fixed: '#261900'
  on-tertiary-fixed-variant: '#5c4200'
  background: '#131314'
  on-background: '#e5e1e3'
  surface-variant: '#353436'
typography:
  headline-lg:
    fontFamily: EB Garamond
    fontSize: 48px
    fontWeight: '700'
    lineHeight: '1.1'
    letterSpacing: -0.02em
  headline-md:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '600'
    lineHeight: '1.2'
  body-lg:
    fontFamily: Courier Prime
    fontSize: 18px
    fontWeight: '400'
    lineHeight: '1.6'
  body-md:
    fontFamily: Courier Prime
    fontSize: 16px
    fontWeight: '400'
    lineHeight: '1.5'
  label-caps:
    fontFamily: Courier Prime
    fontSize: 12px
    fontWeight: '700'
    lineHeight: '1.0'
    letterSpacing: 0.1em
  headline-lg-mobile:
    fontFamily: EB Garamond
    fontSize: 32px
    fontWeight: '700'
    lineHeight: '1.1'
spacing:
  unit: 4px
  gutter: 16px
  margin: 24px
  container-max: 1200px
---

## Brand & Style
The design system is built upon the aesthetic of **Liminal Nostalgia**. It evokes the unsettling calm of an abandoned institutional space—specifically a school at dusk. The emotional response is one of "broken memory": a mixture of comfort from familiar forms and the subtle dread of isolation.

The style is a blend of **Low-Poly Retro** and **Industrial Minimalism**. It mimics the technical constraints of the PS1/PS2 era, utilizing flat shading, dithering patterns, and hard edges. It avoids all modern "slickness," opting instead for a tactile, slightly "crusty" interface that feels like an old computer terminal found in a basement office. Use harsh localized lighting metaphors rather than global ambient light.

## Colors
The palette is dominated by **Corridor Navy**, serving as the void or the shadow. **Fluorescent Sick** acts as the primary light source, mimicking the unnatural hum of overhead tubes. 

- **Backgrounds:** Use `#1a1c2c` for the base. Secondary surfaces use a slightly lifted navy or a dithered pattern.
- **Primary UI Elements:** `#d1d1a5` (Fluorescent Sick) is used for the most important text and borders.
- **Accents:** `#e6af2e` (Memory Amber) highlights interactive "memories" or archival data. `#4bb3b1` (Echo Cyan) is used for systemic feedback.
- **Critical Errors:** `#8b0000` (Wrongness Red) must be used sparingly to indicate a break in the simulation or a forbidden action.

## Typography
The typography system balances institutional authority with the intimacy of a typewriter.

**EB Garamond** is used for all headlines and UI labels. It carries the weight of an official document — archival, cold, and slightly worn. It reads institutional without being technological. **Courier Prime** is used for all long-form text, dialogue, recovered documents, and HUD readouts, grounding the experience in a physical, analog past.

**PROHIBIDO:** Space Grotesk, Inter, Roboto, o cualquier sans-serif moderno. La fuente nunca debe sentirse diseñada — debe sentirse encontrada.

Avoid anti-aliasing effects where possible to maintain the low-resolution aesthetic. Headlines should feel slightly tight, while body text requires generous line height to maintain readability against dark, textured backgrounds.

## Layout & Spacing
The layout follows a **Fixed Grid** model, reminiscent of early 4:3 aspect ratio displays. Layouts should feel intentional and somewhat rigid, as if constrained by a physical monitor.

- **Grid:** A 12-column system for desktop, collapsing to 4 columns for mobile.
- **Margins:** Large, uneven margins are encouraged to create a sense of isolation (e.g., centering a small content block in a large empty field).
- **Rhythm:** Use a 4px baseline. All spacing should be multiples of 4 or 8 to maintain a "pixel-perfect" but chunky alignment.

## Elevation & Depth
Depth is created through **Tonal Layering** and **Harsh Shadows**, not soft blurs. 

- **Layers:** Use subtle shifts in the Navy palette to define surfaces. Higher elevation elements are "lit" by the Fluorescent Sick color.
- **Hard Shadows:** Use 1px or 2px offset shadows with 100% opacity to create a "sticker" or "low-poly" effect. 
- **The "Vignette":** Screen edges should fade into deep black, simulating a localized light source that fails to reach the corners of the room.
- **Texture:** Apply a grain or noise overlay to all surfaces to simulate film grain or an old CRT monitor.

## Shapes
The shape language is strictly **Sharp (0px)**. In this design system, curves are an unnecessary luxury that the hardware cannot afford. 

All containers, buttons, and input fields must have hard 90-degree corners. To add visual interest, use "clipped corners" (45-degree chamfers) on prominent containers to reinforce the low-poly, geometric aesthetic.

## Components
- **Buttons:** Rectangular with a 1px solid border of `Fluorescent Sick`. On hover, the background fills with the border color and the text flips to `Corridor Navy`.
- **Lists:** Items separated by a thin, dotted horizontal line. Use a chevron `>` (Courier Prime) as a cursor for the active selection.
- **Cards:** Use a "Folder" metaphor—sharp rectangles with a small tab at the top left containing a `label-caps` category.
- **Input Fields:** A simple underscore `_` for the cursor. The field is a dark box with a `Fluorescent Sick` bottom border only.
- **Dithering:** Use 50% dither patterns (checkerboard pixels) for disabled states or secondary backgrounds instead of transparency/opacity shifts.
- **The "Wrongness" Indicator:** A rare component consisting of a vibrating, high-contrast red border around a critical piece of information.