import { InjectionKey } from "vue";
import { createStore, Store, useStore as useBaseStore } from "vuex";

import { User } from "./types";

import VuexPersistence from "vuex-persist";

export interface State {
  currentUser: User | null;
  currentVisitor: User | null;
}

export const key: InjectionKey<Store<State>> = Symbol();

export default createStore<State>({
  state: {
    currentUser: null,
    currentVisitor: null,
  },
  mutations: {
    setCurrentUser(state, user: User) {
      state.currentUser = user;
    },
    setCurrentVisitor(state, user: User) {
      state.currentVisitor = user;
    },
  },
  actions: {},
  modules: {},
  plugins: [
    new VuexPersistence({ storage: window.localStorage }).plugin as any,
  ],
});

export function useStore() {
  return useBaseStore(key);
}
