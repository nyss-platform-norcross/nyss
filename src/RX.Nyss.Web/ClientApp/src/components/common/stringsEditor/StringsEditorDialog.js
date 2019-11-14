import React, { useState, Fragment } from 'react';
import Button from '@material-ui/core/Button';
import Dialog from '@material-ui/core/Dialog';
import DialogActions from '@material-ui/core/DialogActions';
import DialogContent from '@material-ui/core/DialogContent';
import DialogTitle from '@material-ui/core/DialogTitle';
import { createForm } from '../../../utils/forms';
import TextInputField from '../../forms/TextInputField';
import { post, get } from '../../../utils/http';
import { useMount } from '../../../utils/lifecycle';
import { Loading } from '../loading/Loading';
import { updateStrings } from '../../../strings';
import Grid from '@material-ui/core/Grid';

export const StringsEditorDialog = ({ stringKey, close }) => {
  const [form, setForm] = useState(null);
  const [languageCodes, setLanguageCodes] = useState([]);

  const currentLanguageCode = "en";

  useMount(() => {
    get(`/api/resources/getString/${encodeURI(stringKey)}`)
      .then(response => {
        const translations = response.value.translations;

        setLanguageCodes(translations.map(t => ({ languageCode: t.languageCode, name: t.name })));

        const translationFields = translations.reduce((prev, current) => ({
          ...prev,
          [`value_${current.languageCode}`]: current.value
        }), {});

        const fields = {
          key: stringKey,
          ...translationFields
        }

        setForm(createForm(fields));
      })
  })

  const handleSave = () => {
    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();

    const dto = {
      key: values.key,
      translations: languageCodes.map(lang => ({
        languageCode: lang.languageCode,
        value: values[`value_${lang.languageCode}`]
      }))
    };

    post("/api/resources/saveString", dto)
      .then(() => {
        updateStrings({
          [stringKey]: values[`value_${currentLanguageCode}`]
        });

        close();
      });
  }

  if (!form) {
    return null;
  }

  return (
    <Dialog open={true} onClose={close} onClick={e => e.stopPropagation()} onKeyDown={e => e.stopPropagation()}>
      <DialogTitle id="form-dialog-title">Edit string resource</DialogTitle>
      <DialogContent style={{ width: 500 }}>
        <Grid container spacing={3}>
          {!form && <Loading />}
          {form && (
            <Fragment>
              <Grid item xs={12}>
                <div>Key:</div>
                <div><b>{stringKey}</b></div>
              </Grid>

              {languageCodes.map(lang => (
                <Grid item xs={12} key={`lang_${lang.languageCode}`}>
                  <TextInputField
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
      {form && <DialogActions>
        <Button onClick={close} color="primary" variant="outlined">
          Cancel
      </Button>
        <Button onClick={handleSave} color="primary" variant="outlined">
          Save
      </Button>
      </DialogActions>}
    </Dialog>
  );
}
