/** @type {import('tailwindcss').Config} */
export const content = [
  './src/**/*.{js,jsx,ts,tsx}',
  'node_modules/daisyui/dist/**/*.js',
  'node_modules/react-daisyui/dist/**/*.js',
];
export const theme = {
  extend: {
    transitionProperty: {
      height: 'height',
    },
    colors: {
      primary: {
        50: '#eff6ff',
        100: '#dbeafe',
        200: '#bfdbfe',
        300: '#93c5fd',
        400: '#60a5fa',
        500: '#3b82f6',
        600: '#2563eb',
        700: '#1d4ed8',
        800: '#1e40af',
        900: '#1e3a8a',
        950: '#172554',
      },
    },
    scale: {
      101: '1.01',
    },
  },
  fontFamily: {
    body: [
      'Inter',
      'ui-sans-serif',
      'system-ui',
      '-apple-system',
      'system-ui',
      'Segoe UI',
      'Roboto',
      'Helvetica Neue',
      'Arial',
      'Noto Sans',
      'sans-serif',
      'Apple Color Emoji',
      'Segoe UI Emoji',
      'Segoe UI Symbol',
      'Noto Color Emoji',
    ],
    sans: [
      'Inter',
      'ui-sans-serif',
      'system-ui',
      '-apple-system',
      'system-ui',
      'Segoe UI',
      'Roboto',
      'Helvetica Neue',
      'Arial',
      'Noto Sans',
      'sans-serif',
      'Apple Color Emoji',
      'Segoe UI Emoji',
      'Segoe UI Symbol',
      'Noto Color Emoji',
    ],
  },
};

export const daisyui = {
  themes: ['light'],
};

export const plugins = [
  // eslint-disable-next-line no-undef,global-require,import/no-extraneous-dependencies
  require('daisyui'),
];
