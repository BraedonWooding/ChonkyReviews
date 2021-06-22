<template>
  <q-toolbar class="bg-primary text-white shadow-2">
    <q-toolbar-title>Visitors</q-toolbar-title>
    <q-btn round color="primary" icon="add" @click="prompt = true" />
  </q-toolbar>
  <q-list bordered>
    <q-item
      v-for="visitor in visitors"
      :key="visitor.email"
      class="q-my-sm"
      clickable
      v-ripple
    >
      <q-item-section avatar>
        <q-avatar color="primary" text-color="white">
          {{ visitor.profileName }}
        </q-avatar>
      </q-item-section>

      <q-item-section>
        <q-item-label>{{ visitor.profileName }}</q-item-label>
        <q-item-label caption lines="1">{{ visitor.email }}</q-item-label>
      </q-item-section>
    </q-item>
  </q-list>
  <q-dialog v-model="prompt">
    <q-card style="min-width: 350px">
      <q-card-section>
        <div class="text-h6">Details</div>
      </q-card-section>

      <q-card-section class="q-pt-none">
        <q-input dense v-model="newVisitor.email" label="Email" autofocus />
        <q-input
          dense
          v-model="newVisitor.profileName"
          label="Name"
          @keyup.enter="(prompt = false), createVisitor()"
        />
      </q-card-section>

      <q-card-actions align="right" class="text-primary">
        <q-btn
          flat
          label="Cancel"
          v-close-popup
          @click="visitor = { email: '', profileName: '' }"
        />
        <q-btn
          flat
          label="Add Visitor"
          v-close-popup
          @click="createVisitor()"
        />
      </q-card-actions>
    </q-card>
  </q-dialog>
</template>

<script lang="ts">
import { onMounted, Ref, ref } from "vue";

import axios from "axios";

import { User } from "../store/types";

export default {
  name: "LayoutDefault",

  setup(): any {
    const visitors: Ref<User[]> = ref([]);
    const newVisitor: Ref<User> = ref({ profileName: "", email: "" });

    async function getVisitors() {
      const resp = await axios.get("/api/visitor/all");
      visitors.value = resp.data;
    }

    async function createVisitor() {
      await axios.post("/api/visitor", newVisitor.value);
      await getVisitors();
    }

    onMounted(getVisitors);

    return {
      visitors,
      prompt: ref(false),
      newVisitor,
      createVisitor,
    };
  },
};
</script>
