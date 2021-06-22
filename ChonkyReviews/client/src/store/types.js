export var AccountType;
(function (AccountType) {
    AccountType[AccountType["ACCOUNT_TYPE_UNSPECIFIED"] = -1] = "ACCOUNT_TYPE_UNSPECIFIED";
    AccountType[AccountType["PERSONAL"] = 0] = "PERSONAL";
    AccountType[AccountType["LOCATION_GROUP"] = 1] = "LOCATION_GROUP";
    AccountType[AccountType["USER_GROUP"] = 2] = "USER_GROUP";
    AccountType[AccountType["ORGANIZATION"] = 3] = "ORGANIZATION";
})(AccountType || (AccountType = {}));
export var StarRating;
(function (StarRating) {
    StarRating[StarRating["STAR_RATING_UNSPECIFIED"] = -1] = "STAR_RATING_UNSPECIFIED";
    StarRating[StarRating["ONE"] = 1] = "ONE";
    StarRating[StarRating["TWO"] = 2] = "TWO";
    StarRating[StarRating["THREE"] = 3] = "THREE";
    StarRating[StarRating["FOUR"] = 4] = "FOUR";
    StarRating[StarRating["FIVE"] = 5] = "FIVE";
})(StarRating || (StarRating = {}));
export const AccountTypes = [
    { type: "Personal", value: AccountType.PERSONAL },
    { type: "Location Group", value: AccountType.LOCATION_GROUP },
    { type: "User Group", value: AccountType.USER_GROUP },
    { type: "Organization", value: AccountType.ORGANIZATION },
];
//# sourceMappingURL=types.js.map