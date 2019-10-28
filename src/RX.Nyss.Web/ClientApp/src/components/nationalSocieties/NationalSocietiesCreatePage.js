import React, { useEffect, useState, Fragment } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as nationalSocietiesActions from './logic/nationalSocietiesActions';
import * as appActions from '../app/logic/appActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SnackbarContent from '@material-ui/core/SnackbarContent';
import SelectField from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import Button from "@material-ui/core/Button";

const NationalSocietiesCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
      contentLanguageId: "",
      countryId: ""
    };

    const validation = {
      name: [validators.required, validators.minLength(3)],
      contentLanguageId: [validators.required],
      countryId: [validators.required]
  };

    return createForm(fields, validation);
  });

  useEffect(() => {
    props.openModule(props.match.path, props.match.params)
  }, [])

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.create(values);
  };

  return (
    <Fragment>
      <Typography variant="h2">Add National Society</Typography>

      {props.loginResponse &&
        <SnackbarContent
          message={props.loginResponse}
        />
      }

      <Form onSubmit={handleSubmit}>
        <TextInputField
          label="National Society name"
          name="name"
          field={form.fields.name}
          autoFocus
        />

        <SelectField
          label="Country"
          name="country"
          field={form.fields.countryId}
        >
          <MenuItem value="1">Norway</MenuItem>
        </SelectField>

        <SelectField
          label="Content language"
          name="contentLanguage"
          field={form.fields.contentLanguageId}
        >
          <MenuItem value="1">Norwegian</MenuItem>
        </SelectField>

        <FormActions>
          <Button onClick={() => props.goToList()}>
            Cancel
          </Button>

          <SubmitButton isFetching={props.isSaving}>
            Save National Society
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

NationalSocietiesCreatePageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  openModule: PropTypes.func,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  contentLanguages: state.appData.contentLanguages,
  isSaving: state.nationalSocieties.formSaving
});

const mapDispatchToProps = {
  getList: nationalSocietiesActions.getList.invoke,
  create: nationalSocietiesActions.create.invoke,
  goToList: nationalSocietiesActions.goToList,
  openModule: appActions.openModule.invoke
};

export const NationalSocietiesCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesCreatePageComponent)
);
