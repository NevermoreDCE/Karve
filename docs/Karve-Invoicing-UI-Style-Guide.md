# Design Specification for Karve Invoicing UI

This document outlines the visual design, accessibility standards, and implementation guidelines for the Karve invoicing system UI. It is optimized for use in a React Single Page Application (SPA) using only HTML, CSS, and SVG.

## Color Palette

### Base Colors (Oceanic Theme)

| Name | RGBA Value | Usage |
| --- | --- | --- |
| Slate Gray | rgba(112, 128, 144, 1) | Background, borders |
| Mist Blue | rgba(176, 196, 222, 1) | Light mode accents |
| Deep Teal | rgba(0, 105, 92, 1) | Dark mode accents |
| Soft Silver | rgba(192, 192, 192, 1) | Text, icons |
| Ocean Blue | rgba(70, 130, 180, 1) | Buttons, links |
| Alert Red | rgba(220, 20, 60, 1) | Error/Overdue indicators |
| Success Green | rgba(60, 179, 113, 1) | Paid status indicators |

#### Light Mode

Background: rgba(245, 245, 245, 1)

Panel: rgba(255, 255, 255, 1)

Text: rgba(33, 33, 33, 1)

Accent: Mist Blue, Ocean Blue

#### Dark Mode

Background: rgba(18, 18, 18, 1)

Panel: rgba(38, 50, 56, 1)

Text: rgba(230, 230, 230, 1)

Accent: Deep Teal, Soft Silver

## Accessibility Requirements

### Color Contrast

All text must meet WCAG AA contrast ratio (4.5:1 minimum).
Status indicators must use both color and icon/text labels.

### Color-Blind Safe Design

Avoid red/green alone for status indicators.
Use shape, border, or icon cues alongside color.

### Keyboard Navigation

All interactive elements must be reachable via Tab.

Use :focus styles for visible focus indicators.

### Screen Reader Support

Use semantic HTML tags (<header>, <nav>, <main>, <section>, <table>, etc.).

Add aria-label, aria-describedby, and role attributes where appropriate.

## Implementation Guidelines (React SPA)

### Technologies

HTML: Semantic structure and accessibility.

CSS: Styling, theming, layout.

SVG: Decorative elements, charts, icons.

### Component Structure

> <App>
>  ├── <Header />
>  ├── <Sidebar />
>  ├── <MainPanel>
>  │    ├── <SummaryCards />
>  │    ├── <InvoiceTable />
>  │    ├── <RevenueChart />
>  │    └── <RecentActivity />
>  └── <Footer />

### Theming

Use CSS variables for color tokens:

> :root {
>   --bg-color: rgba(245, 245, 245, 1);
>   --text-color: rgba(33, 33, 33, 1);
>   --accent-color: rgba(176, 196, 222, 1);
> }
> [data-theme="dark"] {
>   --bg-color: rgba(18, 18, 18, 1);
>   --text-color: rgba(230, 230, 230, 1);
>   --accent-color: rgba(0, 105, 92, 1);
> }

### SVG Usage

Use inline SVG for icons and charts.

Apply fill and stroke via CSS for theming.

Example:
``` svg
<svg viewBox="0 0 24 24" aria-label="Paid">
  <path d="..." fill="var(--accent-color)" />
</svg>
```

### Responsive Design

Use Flexbox and CSS Grid.

Media queries for mobile/tablet breakpoints.