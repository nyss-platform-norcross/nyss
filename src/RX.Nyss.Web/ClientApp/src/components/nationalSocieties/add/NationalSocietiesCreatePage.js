import React, { useEffect, useState } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Layout } from '../../layout/Layout';
import { useLayout } from '../../../utils/layout';
import Typography from '@material-ui/core/Typography';
import * as nationalSocietiesActions from '../logic/nationalSocietiesActions';
import * as appActions from '../../app/logic/appActions';
import TextInputField from '../../forms/TextInputField';
import SnackbarContent from '@material-ui/core/SnackbarContent';
import { validators, createForm } from '../../../utils/forms';
import { Form } from '../../forms/form/Form';
import { FormActions } from '../../forms/formActions/FormActions';
import SelectField from '../../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import * as consts from "../logic/nationalSocietiesConstants";
import { SubmitButton } from '../../forms/submitButton/SubmitButton';
import Button from "@material-ui/core/Button";

const NationalSocietiesCreatePageComponent = (props) => {
  const [form] = useState(() => {
    const fields = {
      name: "",
      contentLanguageId: "0",
      countryId: "0"
    };

    const validation = {
      name: [validators.required, validators.minLength(3)],
      contentLanguageId: [validators.required],
      countryId: [validators.required]
    };

    return createForm(fields, validation);
  });

  useEffect(() => {
    props.updateSiteMap(props.match.path, {})
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
    <div>
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
          <Button onClick={() => props.showList()}>
            Cancel
          </Button>

          <SubmitButton label={"Save National Society"} isFetching={props.isSaving} />
        </FormActions>
      </Form>
    </div>
  );
}

NationalSocietiesCreatePageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  updateSiteMap: PropTypes.func,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  contentLanguages: state.appData.contentLanguages,
  isSaving: state.requests.pending[consts.CREATE_NATIONAL_SOCIETY.name]
});

const mapDispatchToProps = {
  getList: nationalSocietiesActions.getList.invoke,
  create: nationalSocietiesActions.create.invoke,
  showList: nationalSocietiesActions.showList.invoke,
  updateSiteMap: appActions.updateSiteMap
};

export const NationalSocietiesCreatePage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesCreatePageComponent)
);
