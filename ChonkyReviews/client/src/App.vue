<template>
  <q-layout view="lHh Lpr lFf">
    <q-header elevated class="glossy">
      <q-toolbar>
        <q-btn
          flat
          dense
          round
          @click="leftDrawerOpen = !leftDrawerOpen"
          aria-label="Menu"
          icon="menu"
        />
        <q-toolbar-title> Chonky Reviews Manager </q-toolbar-title>

        <div>ChonkyReviews v0.1</div>
      </q-toolbar>
    </q-header>

    <q-drawer v-model="leftDrawerOpen" show-if-above bordered class="bg-grey-2">
      <q-list>
        <q-item-label header>Sections</q-item-label>
        <q-item clickable tag="a" href="#/reviews">
          <q-item-section avatar>
            <q-icon name="reviews" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Reviews</q-item-label>
            <q-item-label caption>Reply to reviews</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable tag="a" href="#/reviews/visitor">
          <q-item-section avatar>
            <q-icon name="rate_review" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Visitor Reviews</q-item-label>
            <q-item-label caption>Make a review</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable tag="a" href="#/users">
          <q-item-section avatar>
            <q-icon name="account_circle" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Users</q-item-label>
            <q-item-label caption>Manage Users</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable tag="a" href="#/visitors">
          <q-item-section avatar>
            <q-icon name="emoji_people" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Visitors</q-item-label>
            <q-item-label caption>Manage Visitors</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable tag="a" href="#/locations">
          <q-item-section avatar>
            <q-icon name="place" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Locations</q-item-label>
            <q-item-label caption>Manage Locations</q-item-label>
          </q-item-section>
        </q-item>
        <q-item clickable tag="a" href="#/accounts">
          <q-item-section avatar>
            <q-icon name="group" />
          </q-item-section>
          <q-item-section>
            <q-item-label>Accounts</q-item-label>
            <q-item-label caption>Manage Accounts</q-item-label>
          </q-item-section>
        </q-item>
      </q-list>
    </q-drawer>

    <q-page-container>
      <router-view />
    </q-page-container>
  </q-layout>
</template>

<script lang="ts">
import { ref } from "vue";

export default {
  name: "LayoutDefault",

  setup(): any {
    return {
      leftDrawerOpen: ref(false),
    };
  },
};
</script>
