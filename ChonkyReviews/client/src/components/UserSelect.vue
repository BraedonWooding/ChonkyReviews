<template>
  <q-select
    v-model="selectedUser"
    use-input
    :options="users"
    option-label="email"
    label-color="white"
    use-chips
    dark
    outlined
    @filter="filterFn"
    style="width: 400px; vertical-align: center"
  >
    <template v-slot:no-option>
      <q-item>
        <q-item-section class="text-white"> No results </q-item-section>
      </q-item>
    </template>
  </q-select>
</template>

<script lang="ts">
import { defineComponent, onMounted, Ref, ref, watch } from "vue";

import axios from "axios";

import { User } from "../store/types";
import { useStore } from "../store";

export default defineComponent({
  name: "user-select",

  props: {
    isVisitor: {
      type: Boolean,
      default: false,
    },
  },

  setup(props): any {
    const users: Ref<User[]> = ref([]);
    const filteredUsers: Ref<User[]> = ref([]);
    const store = useStore();
    const selectedUser: Ref<User | null> = ref(
      props.isVisitor ? store.state.currentVisitor : store.state.currentUser
    );

    watch(selectedUser, () => {
      store.commit(
        `setCurrent${props.isVisitor ? "Visitor" : "User"}`,
        selectedUser.value
      );
    });

    async function getUsers() {
      const resp = await axios.get(
        `/api/${props.isVisitor ? "visitor" : "user"}/all`
      );
      users.value = resp.data;
    }

    const filterFn = (
      val: string,
      update: (arg: () => void) => void
    ) => {
      update(() => {
        const needle = val.toLowerCase();
        filteredUsers.value = users.value.filter(
          (v: User) => !needle || v.email.toLowerCase().indexOf(needle) > -1
        );
      });
    };

    onMounted(getUsers);

    return {
      users,
      filterFn,
      selectedUser,
    };
  },
});
</script>
