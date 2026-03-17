/** @type {import('tailwindcss').Config} */
export default {
  content: [
    "./פרויקט/wwwroot/**/*.{html,js}",
    "./פרויקט/**/*.razor",
    "./פרויקט/Views/**/*.cshtml"
  ],
  theme: {
    extend: {
      colors: {
        // Scoopy Brand Palette
        'primary': '#F4559E',     // Pink
        'secondary': '#F49F46',   // Orange
        'accent': '#C4B88C',      // Warm Tan
        'neutral': '#A68676',     // Brown
        'base-100': '#F2E8E6',    // Rose Background
        'base-content': '#190E0B' // Deep Black Text
      },
      fontFamily: {
        'sans': ['-apple-system', 'BlinkMacSystemFont', 'Segoe UI', 'sans-serif']
      }
    }
  },
  daisyui: {
    themes: [
      {
        scoopy: {
          'primary': '#F4559E',
          'secondary': '#F49F46',
          'accent': '#C4B88C',
          'neutral': '#A68676',
          'base-100': '#F2E8E6',
          'base-200': '#E8DADA',
          'base-300': '#DEC9C9',
          'base-content': '#190E0B',
          'info': '#3B82F6',
          'success': '#10B981',
          'warning': '#F59E0B',
          'error': '#EF4444'
        }
      }
    ],
    darkMode: false,
    styled: true,
    base: true,
    utils: true,
    logs: true
  },
  plugins: [require('daisyui')]
}
