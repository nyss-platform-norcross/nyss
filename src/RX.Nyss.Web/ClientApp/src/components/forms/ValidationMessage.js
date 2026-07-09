import { SnackbarContent } from "@material-ui/core";
import { resetPageContentScroll } from "../layout/Layout";
import { extractString } from "../../strings";

export const ValidationMessage = ({ message }) => {
  return (
    <SnackbarContent
      message={extractString(message)}
      ref={resetPageContentScroll}
      style={{ marginBottom: 25 }}
    />
  );
};
