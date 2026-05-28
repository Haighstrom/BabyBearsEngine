# Text Rendering Analysis

A reference write-up of how text rendering works in BabyBearsEngine today, why small
fonts still look bad even with the GDI+ backend, how the major native rasterizers
(DirectWrite / FreeType / CoreText) compare, whether porting any of them fits the
existing `TextGraphic` / `IFontAtlasGenerator` architecture, and what an *optimal*
text approach looks like.

> Context: GitHub issue #85 added SDF text rendering. This document captures the
> follow-up investigation into general text quality, especially small sizes.

---

## Part 1 — Why GDI small text is actually bad

Three independent, code-evidenced causes. Each one alone softens small text; together
they compound.

### Cause 1 — No hinting / grid-fitting

- `GdiFontAtlasGenerator.cs:72` and `CharacterBitmapGenerator.cs:14,40` use
  `g.TextRenderingHint = TextRenderingHint.AntiAlias`.
- `AntiAlias` is **un-hinted**: glyph outlines are rasterized at their exact
  mathematical position with no grid-fitting. At small sizes a vertical stem that
  should be one crisp pixel column instead smears across two columns at ~50% coverage
  each — the classic "blurry small text" look.
- `AntiAliasGridFit` (hinted grayscale) or `ClearTypeGridFit` (hinted + subpixel) snap
  stems to the pixel grid and are dramatically crisper at 9–13px. This is a **one-line
  change** to test.

### Cause 2 — Atlas is bilinear-filtered + glyphs land on fractional pixels

- `GdiFontAtlasGenerator.cs:28` calls `new DefaultTextureFactory().GenTexture(bitmap)`.
- `DefaultTextureFactory.cs:74` defaults `linearFilter = true` → the glyph atlas is
  sampled with `GL_LINEAR`.
- `TextGraphic` snaps only the **line origin** to integers (`TextGraphic.cs:344`
  `lineTop = MathF.Round(...)`, `:373` `charLeft = MathF.Round(...)`), but then:
  - advances with float accumulation: `charLeft += charAdvance` (`:524`),
  - sizes quads as `renderBox.Size.X * ScaleX` (`:399-401`),
  - offsets by bearings `bearing.X * ScaleX` / `bearing.Y * ScaleY` (`:414-416`).
- Result: every glyph after the first lands on a **fractional pixel boundary**, and
  because the atlas is `GL_LINEAR`, each one is resampled → blur, *even at `ScaleX = 1`*.
- Crisp bitmap text needs **point (nearest) sampling** + **per-glyph integer pixel
  snapping**, not just a snapped line origin.

### Cause 3 — Anti-alias coverage blended without gamma correction

- `r8_texture.frag:20` does a straight `Colour = vec4(alpha) * Input_Colour.Colour;`
  — coverage multiplied directly in sRGB/gamma space.
- The GDI path premultiplies alpha and blends in sRGB too.
- Without gamma-correct blending, anti-aliased edges look too thin/light (or too heavy
  for light-on-dark), which reads as fuzziness. This is the **secondary** cause but
  it's real and visible on thin stems.

### Not a cause: point/pixel size mismatch

- `FontLoader.cs:24,57` constructs `new Font(..., size, ..., GraphicsUnit.Pixel)`, so a
  `FontDefinition` size of `9` means **9 pixels**, not 9 points. There is no hidden DPI
  scaling inflating or shrinking the requested size — ruled out.

---

## Part 2 — Two independent axes

The single most important framing: text quality is governed by **two separate axes**,
and they fail independently.

- **Axis A — the rasterizer.** What turns a font outline into coverage/pixels:
  GDI+ / FreeType / DirectWrite / CoreText / stb_truetype. Owns hinting, AA quality,
  subpixel options.
- **Axis B — the rendering architecture.** How rasterized glyphs reach the screen:
  here, a **static GPU atlas** sampled as **textured quads**. Owns filtering,
  pixel-snapping, gamma, scaling behaviour.

**Swapping the rasterizer (Axis A) will not fix blur caused by Axis B.** Causes 2 and 3
above are Axis-B problems. Porting FreeType while leaving `GL_LINEAR` + fractional
placement in `TextGraphic` would still produce soft small text. Fix Axis B first; it's
free and it's the prerequisite for any rasterizer upgrade to actually show through.

---

## Part 3 — Rasterizer comparison

| | **FreeType** | **DirectWrite** | **CoreText** | **GDI+** (current) | **stb_truetype** |
|---|---|---|---|---|---|
| **Platforms** | All (Win/Linux/macOS/mobile) | Windows only | Apple only | Windows (System.Drawing) | All (single header) |
| **Hinting** | Autohinter + TrueType bytecode; "light" (vertical-only) hinting is the modern grayscale sweet spot | Excellent; ClearType-grade | Excellent | Only in `*GridFit` modes (not the mode currently used) | None |
| **Grayscale AA** | Yes | Yes | Yes | Yes | Only via SDF / DIY |
| **Subpixel / LCD** | Yes (`FT_RENDER_MODE_LCD`) | Yes — ClearType, best-in-class on Windows | Yes | Yes (`ClearTypeGridFit`) | No |
| **.NET binding** | `FreeTypeSharp` (maintained, ships native win/linux/mac libs) or `SharpFont` (old). Pure-managed alternatives: `SixLabors.Fonts`, `Typography` | `Vortice.DirectWrite` / `Vortice.Direct2D1` (SharpDX is dead) | None usable outside MAUI/Xamarin | Built-in (`System.Drawing.Common`) | `StbTrueTypeSharp` (**already referenced in this repo**) |
| **Licensing** | FTL or GPLv2 | OS component, free on Windows | OS component, free on Apple | OS component | Public domain |
| **Natural use** | Atlas rasterizer **or** full shaper/layout | Atlas rasterizer or immediate-mode (D2D) | Atlas rasterizer or immediate-mode (CoreGraphics) | Atlas rasterizer (current) | Atlas rasterizer / SDF source |

Key takeaways:

- **FreeType** is the only cross-platform native rasterizer with great hinting, and it
  has a maintained .NET binding that ships native libraries for all three desktop OSes.
  "Light" hinting is exactly what crisp small grayscale text wants.
- **DirectWrite** gives the best subpixel result *on Windows* but is Windows-only.
- **CoreText** is excellent but effectively unbindable from .NET outside Apple's own
  MAUI/Xamarin stack — not a realistic port target here.
- **stb_truetype** (already in the repo) has no hinting; it's fine as a glyph-outline
  source for SDF generation but not for crisp small grayscale.

---

## Part 4 — Compatibility verdict

What fits the existing architecture, and what doesn't.

### Compatible — as atlas rasterizers (drop-in)

`IFontAtlasGenerator`'s contract is simply **"font → (GL texture + per-glyph
metrics)"**. Any rasterizer that can produce a coverage bitmap plus metrics satisfies
it. So:

- Write e.g. `FreeTypeFontAtlasGenerator : IFontAtlasGenerator`.
- Register it in the hybrid backend system (`EngineConfiguration.RegisterAtlasGenerator`
  / the per-font `FontDefinition.Renderer` selector).
- **`TextGraphic` needs no rework.** All three native rasterizers are drop-in
  compatible at this layer.

### Not wanted — their layout / shaping engines

DirectWrite, CoreText, and FreeType+HarfBuzz all offer full text **layout and shaping**.
We don't want that: `TextLayout` already handles wrapping/alignment, and complex-script
shaping (Arabic, Indic, ligatures) is a separate HarfBuzz topic, not part of this work.
Use these libraries **only as rasterizers**, ignore their layout APIs.

### Fundamentally different — their immediate-mode renderers

DirectWrite→Direct2D and CoreText→CoreGraphics want to paint text **directly onto a
surface** every frame (immediate mode). That bypasses `TextGraphic` and the atlas
entirely, is a fundamentally different architecture, and is painful to bolt onto an
OpenTK/OpenGL pipeline. **Not recommended.**

### Compatible-but-real extension — LCD subpixel AA

Subpixel (LCD/ClearType-style) AA *is* compatible with the atlas approach but is not
free: it needs an **RGB-coverage atlas** (3× horizontal resolution), a **dedicated
shader**, and **dual-source / per-component blending**. It's also **UI-only** (breaks
under rotation/scaling and on non-RGB-stripe displays). Worth it for fixed UI text;
skip it for world/scalable text.

---

## Part 5 — Optimal approach (hybrid by use-case)

The engine already supports **per-font backend selection** (`FontDefinition.Renderer`,
hybrid GDI/SDF). The optimal plan layers onto that.

### Tier 0 — free, do this first (no new dependencies, reversible)

These are pure Axis-B fixes inside the existing GDI path + `TextGraphic`:

1. **Point-sample the text atlas** — pass `linearFilter: false` for the glyph texture
   so it's `GL_NEAREST` (`DefaultTextureFactory.GenTexture`). (SDF stays linear — it
   *needs* linear for distance reconstruction.)
2. **Snap each glyph quad to integer pixels** when scale ≈ 1 (not just the line
   origin) in `TextGraphic`.
3. **GDI `AntiAliasGridFit`** instead of `AntiAlias` in `GdiFontAtlasGenerator` /
   `CharacterBitmapGenerator`.
4. *(Optional)* **Gamma-correct / stem-darken** the coverage blend in the fragment
   shader.

Expectation: Tier 0 alone likely fixes most of the small-text pain, and it's a
**prerequisite** — without it, no rasterizer upgrade will look crisp. Do it first
specifically to *measure how much the rasterizer actually matters* before adding a
native dependency.

### Tier 1 — FreeType rasterizer

- Add `FreeTypeFontAtlasGenerator : IFontAtlasGenerator` via **FreeTypeSharp**.
- Build a **per-size coverage atlas with light hinting**.
- Slots straight into the existing hybrid `Renderer` selector — no `TextGraphic`
  changes.
- **Tier 1b (optional):** LCD subpixel atlas + shader for fixed UI text only.

### Keep SDF for scalable text

SDF stays the right tool for **world-space / zoomed / animated / rotated** text. It is
structurally soft at small fixed sizes (no hinting, lossy at low resolution, rounds
corners) — which is exactly why it's a *complement* to, not a replacement for, a hinted
grayscale path.

### DirectWrite / CoreText — last resort only

Only reach for per-OS native rasterizers if you want the absolute best subpixel result
on each platform. **FreeType + LCD gets ~95% of the way, cross-platform**, for a
fraction of the complexity.

### Honest caveat on the "OS gold standard"

The OS/browser/IDE gold standard *also* uses **subpixel glyph positioning** (caching a
few horizontal phase variants per glyph). Most game UIs and ImGui **skip** this and
still look crisp — they rely on **hinting + pixel-snapping + point-sampling**. So
Tier 0 + Tier 1 realistically closes the perceived gap without implementing subpixel
positioning.

---

## Summary recommendation

1. **Tier 0 first** — point-sample atlas, per-glyph pixel snap, `AntiAliasGridFit`,
   optional gamma. Free, reversible, fixes Axis B.
2. **Tier 1** — `FreeTypeFontAtlasGenerator` (FreeTypeSharp, light hinting) plugged into
   the hybrid selector for crisp small UI text.
3. **Keep SDF** for scalable/world text.
4. **Subpixel / DirectWrite / CoreText** only if you later want per-OS best-in-class —
   not needed to fix the current problem.
