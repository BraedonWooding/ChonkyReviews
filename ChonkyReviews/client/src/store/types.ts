export interface User {
  profileName: string;
  email: string;
  userId: string;
}

export interface Account {
  accountId: string;
  type: AccountType;
  accountName: string;
  name: string;
}

export enum AccountType {
  ACCOUNT_TYPE_UNSPECIFIED = -1,
  PERSONAL = 0,
  LOCATION_GROUP = 1,
  USER_GROUP = 2,
  ORGANIZATION = 3,
}

export enum StarRating {
  STAR_RATING_UNSPECIFIED = -1,
  ONE = 1,
  TWO = 2,
  THREE = 3,
  FOUR = 4,
  FIVE = 5,
}

export interface Review {
  name: string;
  reviewer: {
    displayName: string;
  };
  starRating: StarRating;
  comment: string;
  reviewReply: {
    comment: string;
  };
  locationId: string;
}

export interface Location {
  locationId: string;
  accountId: string;
  name: string;
  locationName: string;
}

export const AccountTypes = [
  { type: "Personal", value: AccountType.PERSONAL },
  { type: "Location Group", value: AccountType.LOCATION_GROUP },
  { type: "User Group", value: AccountType.USER_GROUP },
  { type: "Organization", value: AccountType.ORGANIZATION },
];
