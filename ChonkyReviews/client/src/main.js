import { createApp } from "vue";
import App from "./App.vue";
import router from "./router";
import store, { key } from "./store";
import { Quasar } from "quasar";
import quasarUserOptions from "./quasar-user-options";
createApp(App)
    .use(Quasar, quasarUserOptions)
    .use(store, key)
    .use(router)
    .mount("#app");
//# sourceMappingURL=main.js.map