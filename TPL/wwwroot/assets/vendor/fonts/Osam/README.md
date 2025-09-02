# Osam Font Directory

## Font Files Required
Place the following font files in this directory:

### Required Formats
- **WOFF2** (recommended - best compression)
- **WOFF** (good compression)
- **TTF** (fallback)

### File Names
```
Osam-Light.woff2      (300 weight)
Osam-Regular.woff2    (400 weight)
Osam-Medium.woff2     (500 weight)
Osam-SemiBold.woff2  (600 weight)
Osam-Bold.woff2       (700 weight)
```

## Font Sources
- Download from official font provider
- Ensure you have proper licensing
- Convert to web formats if needed

## Conversion Tools
If you only have TTF files, convert them using:
- [FontSquirrel Webfont Generator](https://www.fontsquirrel.com/tools/webfont-generator)
- [Google Webfonts Helper](https://google-webfonts-helper.herokuapp.com/fonts)
- [Transfonter](https://transfonter.org/)

## Testing
After placing font files:
1. Refresh the page
2. Check browser console for font loading status
3. Verify fonts are applied using browser dev tools

## CSS Integration
Fonts are automatically loaded via:
- `/wwwroot/assets/vendor/fonts/Osam.css`
- Included in main layout
- Applied globally to all elements
