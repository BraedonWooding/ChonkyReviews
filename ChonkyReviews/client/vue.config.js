module.exports = {
  pluginOptions: {
    quasar: {
      importStrategy: "kebab",
      rtlSupport: true,
    },
  },
  publicPath: "",
  devServer: {
    proxy: {
      ".*": {
        target: "http://localhost:5000",
        hostRewrite: "localhost:8080",
      },
    },
  },
  transpileDependencies: ["quasar"],
};
