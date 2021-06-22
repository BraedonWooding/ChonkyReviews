import { createStore, useStore as useBaseStore } from "vuex";
import VuexPersistence from "vuex-persist";
export const key = Symbol();
export default createStore({
    state: {
        currentUser: null,
        currentVisitor: null,
    },
    mutations: {
        setCurrentUser(state, user) {
            state.currentUser = user;
        },
        setCurrentVisitor(state, user) {
            state.currentVisitor = user;
        },
    },
    actions: {},
    modules: {},
    plugins: [
        new VuexPersistence({ storage: window.localStorage }).plugin,
    ],
});
export function useStore() {
    return useBaseStore(key);
}
//# sourceMappingURL=index.js.map