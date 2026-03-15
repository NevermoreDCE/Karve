/** @type {import('tailwindcss').Config} */
export default {
  content: ["./index.html", "./src/**/*.{ts,tsx}"],
  theme: {
    extend: {
      fontFamily: {
        heading: ['"Space Grotesk"', '"Avenir Next"', '"Segoe UI"', 'sans-serif'],
        body: ['"IBM Plex Sans"', '"Trebuchet MS"', '"Segoe UI"', 'sans-serif'],
        mono: ['"IBM Plex Mono"', '"Cascadia Code"', 'Consolas', 'monospace'],
      },
    },
  },
  plugins: [],
}

