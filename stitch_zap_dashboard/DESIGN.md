# Design System Strategy: High-Density Developer Experience

## 1. Overview & Creative North Star: "The Digital Obsidian"
This design system is built for the high-velocity developer. Moving beyond the "generic SaaS" look, we embrace **The Digital Obsidian** as our Creative North Star. Like the volcanic glass, the UI is deep, sharp, and multi-layered. 

We reject the "flat box" aesthetic. Instead, we use intentional asymmetry and high-density information architecture to create an editorial-grade technical environment. By utilizing tonal layering and precise monospace accents, we transform a standard issue tracker into a premium command center that feels both authoritative and frictionless.

---

## 2. Color & Surface Architecture
The palette is rooted in a deep charcoal foundation, using the `surface` tokens to define a sophisticated dark mode that prioritizes long-form legibility and reduced eye strain.

### The "No-Line" Rule
To achieve a high-end feel, **1px solid borders are prohibited for layout sectioning.** Traditional lines clutter the UI. Instead:
- Define boundaries through background shifts (e.g., a `surface-container-low` sidebar against a `surface` main stage).
- Use `8px` to `12px` padding gaps to let the background act as the natural separator.

### Surface Hierarchy & Nesting
Treat the interface as physical layers of obsidian. 
- **Base:** `surface` (#131313) is your ground floor.
- **Nesting:** Place `surface-container-lowest` (#0e0e0e) for recessed areas like code editors. Use `surface-container-high` (#2a2a2a) for elevated elements like active cards. 
- **The Glass Rule:** For floating menus or command palettes, use `surface-container-highest` at 80% opacity with a `20px` backdrop blur to create a "frosted glass" effect that maintains context.

### Signature Textures
Avoid flat primary blocks. For main CTAs and "Hero" moments, apply a subtle linear gradient: 
`linear-gradient(135deg, var(--primary) 0%, var(--primary-container) 100%)`. This adds a "soul" to the indigo accent that solid hex codes lack.

---

## 3. Typography: The Technical Editorial
We pair the Swiss-style precision of **Inter** with the utilitarian soul of a monospace font for data.

- **Display & Headlines:** Use `display-lg` and `headline-md` with tight letter-spacing (-0.02em) to create a bold, editorial impact.
- **The Data Layer:** All Ticket IDs, timestamps, and terminal outputs must use a Monospace font at `label-md` or `label-sm` sizes. This signals "technical accuracy" to the developer.
- **Hierarchy of Urgency:** 
    - **Overdue:** `error` (#ffb4ab)
    - **Soon:** `tertiary` (#ffb783)
    - **On-track:** `secondary` (#c0c1ff) or a custom muted green.

---

## 4. Elevation & Depth: Tonal Layering
We do not use "drop shadows" in the traditional sense. We use **Ambient Light**.

- **The Layering Principle:** Depth is achieved by stacking. A `surface-container-low` card sitting on a `surface` background provides all the "lift" required. 
- **Ambient Shadows:** For high-importance floating elements (Modals, Popovers), use a diffused shadow: 
    - `box-shadow: 0 20px 40px rgba(0, 0, 0, 0.4), 0 0 0 1px rgba(229, 226, 225, 0.05);`
    - The 1px "inner glow" (Ghost Border) mimics the way light catches the edge of a physical screen.
- **The Ghost Border:** If a container requires a border for accessibility, use `outline-variant` (#464554) at **15% opacity**. It should be felt, not seen.

---

## 5. Components

### Buttons
- **Primary:** Gradient fill (Primary to Primary-Container) with `on-primary` text. Roundedness: `md` (0.75rem).
- **Secondary:** Ghost style. `surface-container-high` background with a `Ghost Border`.
- **Tertiary:** Pure text with `primary` color, no background, high-contrast hover state.

### High-Density Cards
Forbid divider lines. Use `surface-container-low` for the card body and `surface-container-high` for the header strip. Separate internal metadata using the spacing scale (e.g., `spacing-4` for logical blocks).

### Navigation & Lists
- **Active State:** Instead of a border, use a "glow" effect—a subtle `primary` box-shadow with a large blur and 10% opacity, paired with a `primary` indicator 2px wide on the leading edge.
- **Spacing:** Use `spacing-2` (0.4rem) for list item density to maximize information display without feeling cramped.

### Status Chips
Use the `tertiary` and `error` tokens with a 10% opacity background fill and a 100% opacity text label. This creates a "soft-signaling" system that highlights urgency without overwhelming the user’s focus.

---

## 6. Do's and Don'ts

### Do
- **Do** use `surface-container-lowest` for background "wells" where code or logs are displayed.
- **Do** lean on `display-md` for empty state headers to maintain an editorial feel.
- **Do** use the `2.5` (0.5rem) spacing unit for consistent internal padding of small components like tooltips.

### Don't
- **Don't** use pure white (#FFFFFF) for text. Always use `on-surface` (#e5e2e1) to prevent "halving" (visual vibration) on dark backgrounds.
- **Don't** use 100% opaque borders. They break the obsidian aesthetic. Always use the "Ghost Border" approach.
- **Don't** use standard "blue" for links. Use the `primary` indigo (#c0c1ff) to maintain the signature brand identity.

### Accessibility Note
While we aim for high-density and subtle depth, always ensure the `on-surface` text maintains at least a 4.5:1 contrast ratio against the `surface-container` tiers. Use `outline` (#908fa0) for secondary icons to ensure they remain visible but subordinate to primary text.