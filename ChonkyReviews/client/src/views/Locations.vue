<template>
  <div>
    <q-toolbar class="bg-primary text-white shadow-2">
      <q-toolbar-title>Locations</q-toolbar-title>
      <UserSelect :isVisitor="false" />
      <q-btn round color="primary" icon="add" @click="prompt = true" />
    </q-toolbar>
    <q-list bordered v-if="hasUser()">
      <q-item
        v-for="location in locations"
        :key="location.locationId"
        class="q-my-sm"
        clickable
        v-ripple
      >
        <q-item-section>
          <q-item-label>{{ location.locationName }}</q-item-label>
          <q-item-label caption lines="1">{{
            location.locationId
          }}</q-item-label>
        </q-item-section>

        <q-item-section>
          <q-toggle
            v-model="location.hasAccess"
            checked-icon="check"
            :disable="true"
            color="red"
            :label="`${
              location.hasAccess
                ? 'User has access'
                : location.hasAccess === null
                ? 'User has access due to Account Access'
                : ''
            }`"
            unchecked-icon="clear"
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
            v-model="newLocation.accountId"
            :options="accounts"
            option-value="accountId"
            emit-value
            option-label="accountName"
            map-options
            label="Account"
            autofocus
          />
          <q-input
            dense
            v-model="newLocation.locationName"
            label="Location Name"
            @keyup.enter="(prompt = false), createLocation()"
          />
        </q-card-section>

        <q-card-actions align="right" class="text-primary">
          <q-btn
            flat
            label="Cancel"
            v-close-popup
            @click="location = { email: '', profileName: '' }"
          />
          <q-btn
            flat
            label="Add Location"
            v-close-popup
            @click="createLocation()"
          />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script lang="ts">
import { onMounted, Ref, ref, watch } from "vue";

import axios from "axios";

import { Account, Location } from "../store/types";
import { useStore } from "../store";

import UserSelect from "../components/UserSelect.vue";

export default {
  name: "LayoutDefault",

  components: {
    UserSelect,
  },

  setup(): any {
    const locations: Ref<Location[]> = ref([]);
    const accounts: Ref<Account[]> = ref([]);
    const newLocation: Ref<Location> = ref({
      locationId: "",
      locationName: "",
      accountId: "",
      name: "",
      hasAccess: false,
    });
    const store = useStore();

    async function getLocations() {
      const resp = await axios.get("/api/location/all");
      locations.value = (await Promise.all(
        resp.data.map(async (x: any) => {
          x.hasAccess = ref(await hasAccess(x.accountId));
          return x;
        })
      )).filter((x: any) => x.hasAccess.value) as Location[];
    }

    watch(store.state, getLocations);

    async function getAccounts() {
      const resp = await axios.get("/api/account/all");
      accounts.value = (await Promise.all(
        resp.data.map(async (x: any) => {
          x.hasAccess = ref(await hasAccess(x.accountId));
          return x;
        })
      )).filter((x: any) => x.hasAccess.value) as Account[];
    }

    async function createLocation() {
      await axios.post("/api/location", newLocation.value);
      await getLocations();
    }

    async function hasAccess(accountId: string) {
      try {
        await axios.get(
          `/api/account/access?accountId=${accountId}&userId=${store.state.currentUser?.userId}`
        );
        return true;
      } catch (e) {
        return false;
      }
    }

    onMounted(getLocations);
    onMounted(getAccounts);

    return {
      locations,
      prompt: ref(false),
      newLocation,
      createLocation,
      hasAccess,
      accounts,
      hasUser: () => store.state.currentUser != null,
    };
  },
};
</script>
