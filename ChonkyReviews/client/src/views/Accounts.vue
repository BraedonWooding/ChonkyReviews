<template>
  <div>
    <q-toolbar class="bg-primary text-white shadow-2">
      <q-toolbar-title>Accounts</q-toolbar-title>
      <UserSelect :isVisitor="false" />
      <q-btn round color="primary" icon="add" @click="prompt = true" />
    </q-toolbar>
    <q-list bordered v-if="hasUser()">
      <q-item
        v-for="account in accounts"
        :key="account.accountId"
        class="q-my-sm"
        clickable
        v-ripple
      >
        <q-item-section>
          <q-item-label>{{ account.accountName }}</q-item-label>
          <q-item-label caption lines="1">{{ account.accountId }}</q-item-label>
        </q-item-section>

        <q-item-section>
          <q-toggle
            v-model="account.hasAccess"
            checked-icon="check"
            color="red"
            unchecked-icon="clear"
            :label="`${
              account.hasAccess
                ? 'User has access'
                : 'User doesn\'t have access'
            }`"
          />
        </q-item-section>
      </q-item>
    </q-list>
    <div class="text-h6" v-else>Select a user to get started (top right).</div>
    <q-dialog v-model="prompt">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">Details</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-select
            filled
            v-model="newAccount.type"
            :options="AccountTypes"
            option-value="value"
            emit-value
            option-label="type"
            map-options
            label="Type"
            autofocus
          />
          <q-input
            dense
            v-model="newAccount.accountName"
            label="Account Name"
            @keyup.enter="(prompt = false), createAccount()"
          />
        </q-card-section>

        <q-card-actions align="right" class="text-primary">
          <q-btn
            flat
            label="Cancel"
            v-close-popup
            @click="account = { email: '', profileName: '' }"
          />
          <q-btn
            flat
            label="Add Account"
            v-close-popup
            @click="createAccount()"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script lang="ts">
import { onMounted, Ref, ref, watch } from "vue";

import axios from "axios";

import { Account, AccountType, AccountTypes } from "../store/types";
import { useStore } from "../store";

import UserSelect from "../components/UserSelect.vue";

export default {
  name: "LayoutDefault",

  components: {
    UserSelect,
  },

  setup(): any {
    const accounts: Ref<Account[]> = ref([]);
    const newAccount: Ref<Account> = ref({
      accountId: "",
      accountName: "",
      type: AccountType.LOCATION_GROUP,
      name: "",
      hasAccess: false,
    });
    const store = useStore();

    async function getAccounts() {
      if (!store.state.currentUser) return;

      const resp = await axios.get("/api/account/all");
      accounts.value = await Promise.all(
        resp.data.map(async (x: any) => {
          x.hasAccess = ref(await hasAccess(x.accountId));
          watch(x.hasAccess, () => toggleAccess(x.accountId));
          return x;
        })
      );
    }

    watch(store.state, getAccounts);

    async function createAccount() {
      await axios.post("/api/account", newAccount.value);
      await getAccounts();
    }

    async function toggleAccess(id: string) {
      ((await hasAccess(id)) === true ? axios.delete : axios.put)(
        `/api/account/access?accountId=${id}&userId=${store.state.currentUser?.userId}`
      );
      await getAccounts();
    }

    async function hasAccess(id: string) {
      try {
        await axios.get(
          `/api/account/access?accountId=${id}&userId=${store.state.currentUser?.userId}`
        );
        return true;
      } catch (e) {
        return false;
      }
    }

    onMounted(getAccounts);

    return {
      accounts,
      prompt: ref(false),
      newAccount,
      createAccount,
      hasAccess,
      toggleAccess,
      AccountTypes,
      hasUser: () => store.state.currentUser != null,
    };
  },
};
</script>
