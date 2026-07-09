import React, { useState, Fragment } from "react";
import { createForm } from "../../../utils/forms";
import TextInputField from "../../forms/TextInputField";
import { post, get } from "../../../utils/http";
import { useMount } from "../../../utils/lifecycle";
import { Loading } from "../loading/Loading";
import { updateStrings } from "../../../strings";
import {
  Grid,
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
} from "@material-ui/core";
import { useDispatch } from "react-redux";
import { stringsUpdated } from "../../app/logic/appActions";
import { stringsDeleted } from "../../app/logic/appActions";
import CheckboxField from "../../forms/CheckboxField";

export const StringsEditorDialog = ({ stringKey, close }) => {
  const [form, setForm] = useState(null);
  const [languageCodes, setLanguageCodes] = useState([]);
  const dispatch = useDispatch();

  const currentLanguageCode = "en";

  useMount(() => {
    get(`/api/resources/getString/${encodeURI(stringKey)}`).then((response) => {
      const translations = response.value.translations;

      setLanguageCodes(
        translations.map((t) => ({
          languageCode: t.languageCode,
          name: t.name,
        })),
      );

      const translationFields = translations.reduce(
        (prev, current) => ({
          ...prev,
          [`value_${current.languageCode}`]: current.value,
        }),
        {},
      );

      const fields = {
        key: stringKey,
        needsImprovement: response.value.needsImprovement,
        ...translationFields,
      };

      setForm(createForm(fields));
    });
  });

  const handleSave = () => {
    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    const dto = {
      key: values.key,
      needsImprovement: values.needsImprovement,
      translations: languageCodes.map((lang) => ({
        languageCode: lang.languageCode,
        value: values[`value_${lang.languageCode}`],
      })),
    };

    post("/api/resources/saveString", dto).then(() => {
      updateStrings({
        [values.key]: {
          needsImprovement: values.needsImprovement,
          value: values[`value_${currentLanguageCode}`],
        },
      });

      dispatch(
        stringsUpdated(
          dto.key,
          dto.translations.reduce(
            (prev, current) => ({
              ...prev,
              [current.languageCode]: current.value,
            }),
            {},
          ),
        ),
      );
      close();
    });
  };

  const handleDelete = () => {
    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    const dto = {
      key: values.key,
    };

    post("/api/resources/deleteString", dto).then(() => {
      dispatch(stringsDeleted(dto.key));
      close();
    });
  };

  if (!form) {
    return null;
  }

  const handleKeyDown = (e) => {
    e.stopPropagation();

    if (e.key === "Escape") {
      close();
    }

    if (e.key === "Enter") {
      handleSave();
    }
  };

  return (
    <Dialog
      open={true}
      onClose={close}
      onClick={(e) => e.stopPropagation()}
      onKeyDown={handleKeyDown}
    >
      <DialogTitle id="form-dialog-title">Edit string resource</DialogTitle>
      <DialogContent style={{ width: 500 }}>
        <Grid container spacing={2}>
          {!form && <Loading />}
          {form && (
            <Fragment>
              <Grid item xs={12}>
                <TextInputField
                  label="Key"
                  name="key"
                  field={form.fields.key}
                />
              </Grid>

              {languageCodes.map((lang, index) => (
                <Grid item xs={12} key={`lang_${lang.languageCode}`}>
                  <TextInputField
                    autoFocus={index === 0}
                    label={lang.name}
                    name={`value_${lang.languageCode}`}
                    field={form.fields[`value_${lang.languageCode}`]}
                  />
                </Grid>
              ))}
            </Fragment>
          )}
        </Grid>
        <br />
      </DialogContent>
      {form && (
        <DialogActions>
          <CheckboxField
            name="needsImprovement"
            label="Needs improvement"
            field={form.fields.needsImprovement}
          />
          <Button onClick={handleDelete} color="secondary" variant="text">
            Delete
          </Button>
          <Button onClick={close} color="primary" variant="outlined">
            Cancel
          </Button>
          <Button onClick={handleSave} color="primary" variant="contained">
            Save
          </Button>
        </DialogActions>
      )}
    </Dialog>
  );
};
