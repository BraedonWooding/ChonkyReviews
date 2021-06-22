const plugins = [require("autoprefixer")];

if (process.env.QUASAR_RTL) {
  // eslint-disable-next-line
  plugins.push(require("postcss-rtl")({}));
}

module.exports = {
  plugins,
};
