import React, { useEffect, useState, Fragment } from 'react';
import { connect } from "react-redux";
import { useLayout } from '../../utils/layout';
import { validators, createForm } from '../../utils/forms';
import * as nationalSocietyUsersActions from './logic/nationalSocietyUsersActions';
import Layout from '../layout/Layout';
import Form from '../forms/form/Form';
import FormActions from '../forms/formActions/FormActions';
import SubmitButton from '../forms/submitButton/SubmitButton';
import Typography from '@material-ui/core/Typography';
import TextInputField from '../forms/TextInputField';
import SelectInput from '../forms/SelectField';
import MenuItem from "@material-ui/core/MenuItem";
import SnackbarContent from '@material-ui/core/SnackbarContent';
import Button from "@material-ui/core/Button";
import { Loading } from '../common/loading/Loading';
import { nationalSocietyUserTypes } from "./logic/nationalSocietyUserTypes";
import { useMount } from '../../utils/lifecycle';
import { strings } from '../../strings';
import Grid from '@material-ui/core/Grid';

const NationalSocietyUsersEditPageComponent = (props) => {
  const [form, setForm] = useState(null);

  useMount(() => {
    props.openEdition(props.nationalSocietyUserId);
  });

  useEffect(() => {
    if (!props.data) {
      return;
    }

    const fields = {
      id: props.data.id,
      name: props.data.name,
      apiKey: props.data.apiKey,
      gatewayType: props.data.gatewayType.toString()
    };

    const validation = {
      name: [validators.required, validators.minLength(1), validators.maxLength(100)],
      apiKey: [validators.required, validators.minLength(1), validators.maxLength(100)],
      gatewayType: [validators.required]
    };

    setForm(createForm(fields, validation));
  }, [props.data, props.match]);

  const handleSubmit = (e) => {
    e.preventDefault();

    if (!form.isValid()) {
      return;
    };

    const values = form.getValues();
    props.edit(props.nationalSocietyId, {
      id: values.id,
      name: values.name,
      apiKey: values.apiKey,
      gatewayType: values.gatewayType
    });
  };

  if (props.isFetching || !form) {
    return <Loading />;
  }

  return (
    <Fragment>
      <Typography variant="h2">Edit User</Typography>

      {props.error &&
        <SnackbarContent
          message={props.error}
        />
      }

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={3}>
          <Grid item xs={12}>
            <TextInputField
              label="Name"
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label="API key"
              name="apiKey"
              field={form.fields.apiKey}
            />
          </Grid>

          <Grid item xs={12}>
            <SelectInput
              label="Gateway type"
              name="gatewayType"
              field={form.fields.gatewayType}
            >
              {nationalSocietyUserTypes.map(type => (
                <MenuItem
                  key={`gatewayType${type}`}
                  value={type}>
                  {strings(`nationalSocietyUser.type.${type.toLowerCase()}`)}
                </MenuItem>
              ))}
            </SelectInput>
          </Grid>
        </Grid>

        <FormActions>
          <Button onClick={() => props.goToList(props.nationalSocietyId)}>
            Cancel
          </Button>

          <SubmitButton isFetching={props.isSaving}>
            Save User
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
}

NationalSocietyUsersEditPageComponent.propTypes = {
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyUserId: ownProps.match.params.nationalSocietyUserId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  isFetching: state.nationalSocietyUsers.formFetching,
  isSaving: state.nationalSocietyUsers.formSaving,
  data: state.nationalSocietyUsers.formData,
  error: state.nationalSocietyUsers.formError
});

const mapDispatchToProps = {
  openEdition: nationalSocietyUsersActions.openEdition.invoke,
  edit: nationalSocietyUsersActions.edit.invoke,
  goToList: nationalSocietyUsersActions.goToList
};

export const NationalSocietyUsersEditPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietyUsersEditPageComponent)
);
