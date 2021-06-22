<template>
  <div>
    <q-toolbar class="bg-primary text-white shadow-2">
      <q-toolbar-title>Reviews</q-toolbar-title>
      <UserSelect :isVisitor="true" />
    </q-toolbar>
    <q-list bordered>
      <q-item
        v-for="location in locations"
        :key="location.reviewId"
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

        <q-item-section />
        <q-item-section />

        <q-item-section>
          <q-btn
            color="primary"
            @click="getCurrentReview(location.locationId), (prompt = true)"
            :label="!location.review ? 'Add Review' : 'Edit Review'"
          />
        </q-item-section>
      </q-item>
    </q-list>
    <q-dialog v-model="prompt">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">Review</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-rating v-model="newReview.starRating" :max="5" size="32px" />
          <q-input
            type="textarea"
            dense
            v-model="newReview.comment"
            label="Comment"
          />
        </q-card-section>

        <q-card-actions align="right" class="text-primary">
          <q-btn flat label="Cancel" v-close-popup @click="newReview = {}" />
          <q-btn flat label="Save Review" v-close-popup @click="saveReview()" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script lang="ts">
import { onMounted, Ref, ref } from "vue";

import axios from "axios";

import { Location, Review, StarRating } from "../store/types";
import { useStore } from "../store";

import UserSelect from "../components/UserSelect.vue";

export default {
  name: "LayoutDefault",

  components: {
    UserSelect,
  },

  setup(): any {
    const locations: Ref<Location[]> = ref([]);
    const newReview: Ref<Review> = ref({
      locationId: "",
      comment: "",
      starRating: StarRating.STAR_RATING_UNSPECIFIED,
      name: "",
      reviewer: {
        displayName: "",
      },
      reviewReply: {
        comment: "",
      },
    });
    const store = useStore();

    async function getLocations() {
      const resp = await axios.get("/api/location/all");
      locations.value = await Promise.all(
        resp.data.map(async (x: any) => {
          x.review = ref(await getReview(x.locationId));
          return x;
        })
      );
    }

    async function getReview(locationId: string) {
      try {
        return (
          await axios.get(
            `/api/review?locationId=${locationId}&userId=${store.state.currentUser?.email}`
          )
        ).data;
      } catch (e) {
        return false;
      }
    }

    async function getCurrentReview(locationId: string) {
      newReview.value = (await getReview(locationId)) || {
        locationId: locationId,
        comment: "",
        starRating: StarRating.STAR_RATING_UNSPECIFIED,
        name: "",
        reviewer: {
          displayName: store.state.currentVisitor?.profileName,
        },
        reviewReply: {
          comment: "",
        },
      };
      newReview.value.locationId = locationId;
    }

    async function saveReview() {
      await axios.post(`/api/review?locationId=${newReview.value.locationId}`, {
        ...newReview.value,
        reviewer: {
          ...store.state.currentUser,
          userId: store.state.currentUser?.email,
        },
      });
      getLocations();
    }

    onMounted(getLocations);

    return {
      locations,
      prompt: ref(false),
      newReview,
      saveReview,
      getReview,
      getCurrentReview,
    };
  },
};
</script>
