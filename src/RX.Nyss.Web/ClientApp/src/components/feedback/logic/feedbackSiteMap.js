import { accessMap } from "../../../authentication/accessMap";
import { stringKeys, strings } from "../../../strings";

export const feedbackSiteMap = [
  {
    path: "/feedback",
    title: () => strings(stringKeys.feedback.send),
    access: accessMap.feedback.send,
  },
];
