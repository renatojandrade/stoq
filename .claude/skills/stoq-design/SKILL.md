---
name: stoq-design
description: Use this skill to generate well-branded interfaces and assets for Stoq, either for production or throwaway prototypes/mocks/etc. Contains essential design guidelines, colors, type, fonts, assets, and UI kit components for prototyping. Stoq is a mobile-first smart inventory & grocery manager for households (React Native / Expo, Material Design 3, Brazilian Portuguese).
user-invocable: true
---

Read the `README.md` file within this skill, and explore the other available files (`colors_and_type.css` for tokens, `ui_kits/mobile/` for the interactive component recreation, `preview/` for specimen cards, `assets/` for the logo).

If creating visual artifacts (slides, mocks, throwaway prototypes, etc), copy assets out and create static HTML files for the user to view — load Roboto from Google Fonts and Material Community Icons from `@mdi/font` (CDN), reuse the tokens in `colors_and_type.css`, and pull components from `ui_kits/mobile/`. If working on production code, copy assets and read the rules here to become an expert in designing with this brand (React Native Paper / MD3, pt-BR copy, the green palette, sentence-case voice, no emoji).

Key reminders specific to Stoq:
- **Brand color is green** (`#2D6A4F` primary). Keep accent purposeful over a large neutral field.
- The shipping app leaves many MD3 tokens at their **purple baseline** — this system replaces them with green-tinted neutrals. Prefer the tokens in `colors_and_type.css`.
- Copy is **Brazilian Portuguese, sentence case, second-person, no emoji**, with the `…` ellipsis character.
- Icons are **Material Community Icons** (`mdi-*`). Never hand-draw icons or use emoji.

If the user invokes this skill without any other guidance, ask them what they want to build or design, ask some questions, and act as an expert designer who outputs HTML artifacts _or_ production code, depending on the need.
