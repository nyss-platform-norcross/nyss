import React, { useState, Fragment } from "react";
import {
  Button,
  Dialog,
  DialogActions,
  DialogContent,
  DialogTitle,
  Grid,
} from "@material-ui/core";
import { createForm } from "../../../utils/forms";
import TextInputField from "../../forms/TextInputField";
import { post, get } from "../../../utils/http";
import { useMount } from "../../../utils/lifecycle";
import { Loading } from "../loading/Loading";
import { useDispatch } from "react-redux";
import {
  smsStringsDeleted,
  smsStringsUpdated,
} from "../../app/logic/appActions";
import TextInputWithCharacterCountField from "../../forms/TextInputWithCharacterCountField";

export const SmsStringsEditorDialog = ({ stringKey, close }) => {
  const [form, setForm] = useState(null);
  const [languageCodes, setLanguageCodes] = useState([]);
  const dispatch = useDispatch();

  const currentLanguageCode = "en";

  useMount(() => {
    get(`/api/resources/getSmsString/${encodeURI(stringKey)}`).then(
      (response) => {
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
          ...translationFields,
        };

        setForm(createForm(fields));
      },
    );
  });

  const handleSave = () => {
    if (!form.isValid()) {
      return;
    }

    const values = form.getValues();

    const dto = {
      key: values.key,
      translations: languageCodes.map((lang) => ({
        languageCode: lang.languageCode,
        value: values[`value_${lang.languageCode}`],
      })),
    };

    post("/api/resources/saveSmsString", dto).then(() => {
      dispatch(
        smsStringsUpdated(
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

    post("/api/resources/deleteSmsString", dto).then(() => {
      dispatch(smsStringsDeleted(dto.key));
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
                  <TextInputWithCharacterCountField
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
