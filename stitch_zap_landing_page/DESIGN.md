# Design System Documentation: The Deep Kinetic

## 1. Overview & Creative North Star

This design system is built upon a Creative North Star we call **"The Deep Kinetic."** 

Unlike standard SaaS platforms that rely on rigid grids and clinical borders, this system treats the interface as a dark, pressurized environment where light and depth define functionality. It borrows the utilitarian precision of developer tools like Vercel and Linear but injects a high-end editorial soul. 

The aesthetic is defined by **intentional asymmetry** and **tonal layering**. We avoid the "template" look by using exaggerated whitespace and overlapping elements that feel like they are floating in a void. We do not use lines to separate ideas; we use the weight of the dark space itself and the subtle glow of the "Zap" purple accent to guide the eye.

---

## 2. Colors

The palette is anchored in a deep, nocturnal foundation, utilizing Material-inspired naming conventions to define role-based logic.

*   **Foundation:** The core background is `surface` (#131313). For high-impact, immersive sections (like Hero footers), we drop to `surface_container_lowest` (#0e0e0e).
*   **The Accent (The Spark):** Our `primary` color (#c0c1ff) is a high-luminance lavender-blue. It should be used sparingly but with high intent—for CTAs, active states, and "kinetic" highlights.

### The "No-Line" Rule
Standard UI relies on 1px borders to define sections. **This design system prohibits the use of solid 1px borders for layout sectioning.** 
Boundaries must be defined solely through background color shifts. For example, a sidebar should be `surface_container_low` (#1c1b1b) sitting against a `surface` (#131313) main body. The contrast is felt, not seen.

### Surface Hierarchy & Nesting
Treat the UI as a series of physical layers. Use the `surface_container` tiers to create depth:
1.  **Backdrop:** `surface_container_lowest` (#0e0e0e)
2.  **Base Layer:** `surface` (#131313)
3.  **Floating Cards:** `surface_container_high` (#2a2a2a)
4.  **Active/Hover States:** `surface_container_highest` (#353534)

### The "Glass & Gradient" Rule
To ensure a premium feel, main CTAs and Hero backgrounds should utilize a subtle gradient: `primary` (#c0c1ff) transitioning into `primary_container` (#8083ff) at a 135-degree angle. Floating elements should use a `backdrop-blur` of 12px to 20px with a semi-transparent `surface_variant` (#353534 at 40% opacity) to create a "frosted obsidian" effect.

---

## 3. Typography

The typography strategy pairs architectural structure with clinical precision.

*   **Display & Headlines (Manrope):** We use Manrope for all `display` and `headline` levels. Its geometric but slightly rounded nature feels modern and authoritative.
    *   *Styling Tip:* For `display-lg` (3.5rem), use a slight negative letter-spacing (-0.02em) to create a "tight" editorial feel.
*   **Body & Labels (Inter):** We use Inter for all functional reading. Inter’s tall x-height ensures readability against high-contrast dark backgrounds.
    *   *Styling Tip:* For `label-sm` (0.6875rem), use `on_surface_variant` (#c7c4d7) and increase letter-spacing to 0.05em to ensure the small text remains legible and "breathable."

---

## 4. Elevation & Depth

We move away from the traditional shadow-heavy "Material" look in favor of **Tonal Layering.**

### The Layering Principle
Depth is achieved by "stacking" container tiers. A `surface_container_lowest` card placed on a `surface` section creates a natural "sunken" effect. Conversely, a `surface_container_high` card on a `surface` section creates a "lifted" effect.

### Ambient Shadows
When a floating effect is required (e.g., Modals or Popovers), shadows must be extra-diffused. 
*   **Blur:** 40px to 60px.
*   **Opacity:** 4% - 8%.
*   **Color:** Use the `on_surface` (#e5e2e1) tint rather than pure black to mimic the way light scatters in a dark room.

### The "Ghost Border" Fallback
If a border is absolutely necessary for accessibility, use a **Ghost Border**. Use the `outline_variant` (#464554) at **15% opacity**. This provides a hint of structure without breaking the seamless, "no-line" aesthetic.

---

## 5. Components

### Buttons
*   **Primary:** Fill with the `primary`-to-`primary_container` gradient. Text is `on_primary` (#1000a9). Use `full` rounding (9999px) for a "pill" look that feels more modern than a rectangle.
*   **Secondary:** Ghost style. No fill. A Ghost Border (outline-variant at 20%) with `on_surface` text.
*   **Tertiary:** Text-only using `primary` (#c0c1ff).

### Cards & Lists
*   **Rounding:** Use the `md` scale (0.75rem / 12px) for cards.
*   **Spacing:** Forbid divider lines. Use `spacing-6` (2rem) or `spacing-8` (2.75rem) between list items to let the content breathe.
*   **Leading Elements:** Use `primary_container` with low opacity for icon backdrops to create a soft glow behind icons.

### Input Fields
*   **Surface:** Use `surface_container_lowest` (#0e0e0e).
*   **Focus State:** Shift the background to `surface_container_low` and apply a 1px Ghost Border using `primary` at 40% opacity. 

### Additional Component: The "Kinetic" Badge
For developer status (e.g., "Build Succeeded"), use a `tertiary_fixed` (#ffdcc5) background with `on_tertiary_fixed` (#301400) text. This warm ochre breaks the cold blue/gray palette and signals "human" interaction or system status.

---

## 6. Do's and Don'ts

### Do
*   **Do** embrace verticality. Use `spacing-16` (5.5rem) or `spacing-20` (7rem) between major landing page sections.
*   **Do** use asymmetrical layouts. Off-setting a headline to the left and a card to the right creates a sophisticated, non-corporate rhythm.
*   **Do** use `on_surface` (#e5e2e1) for text instead of pure white (#FFFFFF) to reduce eye strain in dark mode.

### Don't
*   **Don't** use 100% opaque, high-contrast borders. It makes the UI feel "boxed in" and dated.
*   **Don't** use standard drop shadows. If the element doesn't feel like it's floating through tonal shift, reconsider the layout.
*   **Don't** crowd the interface. If you feel like you need a divider line, you probably just need more whitespace (`spacing-4` or `spacing-6`).