<template>
  <div>
    <q-toolbar class="bg-primary text-white shadow-2">
      <q-toolbar-title>Reviews</q-toolbar-title>
      <UserSelect :isVisitor="false" />
    </q-toolbar>
    <q-list bordered v-if="hasUser()">
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

        <q-item-section> </q-item-section>
        <q-item-section> </q-item-section>

        <q-item-section>
          <q-btn
            color="primary"
            @click="
              (locationForReviews = location),
                (showReviews = true),
                reloadReviews(location.locationId)
            "
            label="Open Reviews"
          />
        </q-item-section>
      </q-item>
    </q-list>
    <div class="text-h6" v-else>Select a user to get started (top right).</div>
    <q-dialog v-model="showReviews">
      <q-card>
        <q-img src="https://cdn.quasar.dev/img/chicken-salad.jpg" />

        <q-card-section>
          <div class="row no-wrap items-center">
            <div class="col text-h6 ellipsis">
              {{ locationForReviews.locationName }}
            </div>
            <div
              class="
                col-auto
                text-grey text-caption
                q-pt-md
                row
                no-wrap
                items-center
              "
            ></div>
          </div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <div class="text-caption text-grey">
            Descriptions aren't supported yet...
          </div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-scroll-area
            :thumb-style="thumbStyle"
            :bar-style="barStyle"
            style="max-width: 450px; height: 50vh"
          >
            <q-card
              v-for="review in reviews"
              :key="locationForReviews.locationId + review.reviewId"
              class="q-pa-xs"
            >
              <div class="text-h6">
                {{ review.reviewer.displayName || "Anonymous" }}
              </div>
              <q-rating
                readonly
                v-model="review.starRating"
                :max="5"
                size="32px"
                style="margin-top: 10px"
              />
              <div class="text-caption" style="margin-top: 10px">
                {{ review.comment || "No comment" }}
              </div>
              <div class="text-caption text-grey" style="margin-top: 10px">
                {{ review.reviewReply?.comment || "No Reply" }}
              </div>
              <q-btn
                color="primary"
                @click="(newReply = review), (prompt = true)"
                :label="
                  !review.reviewReply?.comment ? 'Add Reply' : 'Edit Reply'
                "
              />
            </q-card>
          </q-scroll-area>
        </q-card-section>

        <q-separator />

        <q-card-actions align="right">
          <q-btn v-close-popup flat color="primary" label="Close" />
        </q-card-actions>
      </q-card>
    </q-dialog>
    <q-dialog v-model="prompt">
      <q-card style="min-width: 350px">
        <q-card-section>
          <div class="text-h6">Review</div>
        </q-card-section>

        <q-card-section class="q-pt-none">
          <q-input
            type="textarea"
            dense
            v-model="newReply.reviewReply.comment"
            label="Comment"
          />
        </q-card-section>

        <q-card-actions align="right" class="text-primary">
          <q-btn flat label="Cancel" v-close-popup @click="newReply = {}" />
          <q-btn flat label="Save Reply" v-close-popup @click="saveReply()" />
        </q-card-actions>
      </q-card>
    </q-dialog>
  </div>
</template>

<script lang="ts">
import { onMounted, Ref, ref, watch } from "vue";

import axios from "axios";

import { Location, Review, StarRating } from "../store/types";
import UserSelect from "../components/UserSelect.vue";
import { useStore } from 'vuex';

export default {
  name: "LayoutDefault",

  components: {
    UserSelect,
  },

  setup(): any {
    const locations: Ref<Location[]> = ref([]);
    const newReply: Ref<Review> = ref({
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
    const reviews: Ref<Review[]> = ref([]);
    const store = useStore();

    async function saveReply() {
      await axios.post(
        `/api/review?locationId=${newReply.value.locationId}`,
        newReply.value
      );
      getLocations();
      reloadReviews(newReply.value.locationId);
    }

    async function reloadReviews(locationId: string) {
      const resp = await axios.get(
        `/api/review/forLocation?locationid=${locationId}`
      );
      reviews.value = resp.data;
    }

    watch(store.state, getLocations);

    async function getLocations() {
      if (!store.state.currentUser) {
        return;
      }

      const resp = await axios.get(`/api/location/forUser?userId=${store.state.currentUser.userId}`);
      locations.value = await Promise.all(
        resp.data.map(async (x: any) => {
          return x;
        })
      );
    }

    onMounted(getLocations);

    return {
      locations,
      prompt: ref(false),
      newReply,
      locationForReviews: ref(null),
      showReviews: ref(false),
      reviews,
      reloadReviews,
      saveReply,
      hasUser: () => store.state.currentUser != null,
    };
  },
};
</script>
