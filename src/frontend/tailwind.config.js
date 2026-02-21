module.exports = {
  darkMode: 'class',
  content: [
    './node_modules/flowbite/**/*.js',
    './components/**/*.{vue,js,ts}',
    './layouts/**/*.vue',
    './pages/**/*.vue',
    './composables/**/*.{js,ts}',
    './plugins/**/*.{js,ts}',
    './App.{js,ts,vue}',
    './app.{js,ts,vue}',
    './error.{js,ts,vue}',
  ],
  plugins: [require('flowbite/plugin')],
}