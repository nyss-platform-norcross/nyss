import { useState, Fragment, useMemo, useCallback } from "react";
import { connect } from "react-redux";
import { withLayout } from "../../utils/layout";
import { validators, createForm, useCustomErrors } from "../../utils/forms";
import * as nationalSocietyUsersActions from "./logic/nationalSocietyUsersActions";
import Layout from "../layout/Layout";
import Form from "../forms/form/Form";
import FormActions from "../forms/formActions/FormActions";
import SubmitButton from "../common/buttons/submitButton/SubmitButton";
import CancelButton from "../common/buttons/cancelButton/CancelButton";
import TextInputField from "../forms/TextInputField";
import PhoneInputField from "../forms/PhoneInputField";
import { Loading } from "../common/loading/Loading";
import { useMount } from "../../utils/lifecycle";
import SelectField from "../forms/SelectField";
import * as roles from "../../authentication/roles";
import { stringKeys, strings, stringsFormat } from "../../strings";
import { MenuItem, Grid } from "@material-ui/core";
import { sexValues } from "./logic/nationalSocietyUsersConstants";
import { ValidationMessage } from "../forms/ValidationMessage";
import { getBirthDecades, parseBirthDecade } from "../../utils/birthYear";
import { trackPageView } from "../../utils/appInsightsHelper";

const NationalSocietyUsersEditPageComponent = ({
  nationalSocietyId,
  nationalSocietyUserId,
  data,
  organizations,
  isFetching,
  isSaving,
  error,
  callingUserRoles,
  modems,
  directionRtl,
  countryCode,
  openEdition,
  edit,
  goToList,
}) => {
  const [birthDecades] = useState(getBirthDecades());
  const [selectedRole, setRole] = useState(null);

  useMount(() => {
    openEdition(nationalSocietyUserId);

    // Track page view
    trackPageView("NationalSocietyUsersEditPage");
  });

  const hasAnyRole = useCallback(
    (...roles) =>
      callingUserRoles.some((userRole) =>
        roles.some((role) => role === userRole),
      ),
    [callingUserRoles],
  );

  const canSelectModem = useMemo(
    () =>
      (selectedRole === roles.Manager ||
        selectedRole === roles.TechnicalAdvisor ||
        selectedRole === roles.HeadSupervisor ||
        selectedRole === roles.Supervisor) &&
      modems.length > 0,
    [modems, selectedRole],
  );

  const form = useMemo(() => {
    if (!data) {
      return null;
    }

    const fields = {
      id: data.id,
      nationalSocietyId: parseInt(nationalSocietyId),
      role: data.role,
      email: data.email,
      name: data.name,
      phoneNumber: data.phoneNumber,
      additionalPhoneNumber: data.additionalPhoneNumber,
      organization: data.organization,
      decadeOfBirth: data.decadeOfBirth ? data.decadeOfBirth.toString() : "",
      projectId: data.projectId ? data.projectId.toString() : "",
      organizationId: data.organizationId ? data.organizationId.toString() : "",
      sex: data.sex ? data.sex : "",
      headSupervisorId: data.headSupervisorId
        ? data.headSupervisorId.toString()
        : "",
      modemId: data.modemId ? data.modemId.toString() : "",
    };

    const validation = {
      email: [validators.required, validators.email, validators.maxLength(100)],
      name: [validators.required, validators.maxLength(100)],
      phoneNumber: [
        validators.required,
        validators.maxLength(20),
        validators.phoneNumber,
      ],
      additionalPhoneNumber: [validators.maxLength(20), validators.phoneNumber],
      organization: [
        validators.requiredWhen((f) => f.role === roles.DataConsumer),
        validators.maxLength(100),
      ],
      decadeOfBirth: [
        validators.requiredWhen(
          (f) => f.role === roles.Supervisor || f.role === roles.HeadSupervisor,
        ),
      ],
      sex: [
        validators.requiredWhen(
          (f) => f.role === roles.Supervisor || f.role === roles.HeadSupervisor,
        ),
      ],
      projectId: [
        validators.requiredWhen(
          (f) => f.role === roles.Supervisor || f.role === roles.HeadSupervisor,
        ),
      ],
      organizationId: [
        validators.requiredWhen(
          (f) =>
            f.role === roles.Coordinator || f.role === roles.GlobalCoordinator,
        ),
      ],
      modemId: [validators.requiredWhen((_) => canSelectModem)],
    };

    setRole(data.role);

    return createForm(fields, validation);
  }, [data, nationalSocietyId, canSelectModem]);

  useCustomErrors(form, error);

  const canChangeOrganization = useMemo(
    () =>
      hasAnyRole(roles.Administrator) && selectedRole !== roles.DataConsumer,
    [hasAnyRole, selectedRole],
  );

  const handleSubmit = useCallback(
    (e) => {
      e.preventDefault();

      if (!form.isValid()) {
        return;
      }

      const values = form.getValues();

      edit(nationalSocietyId, {
        ...values,
        organizationId: !!values.organizationId
          ? parseInt(values.organizationId)
          : null,
        projectId: !!values.projectId ? parseInt(values.projectId) : null,
        decadeOfBirth: !!values.decadeOfBirth
          ? parseInt(values.decadeOfBirth)
          : null,
        headSupervisorId: !!values.headSupervisorId
          ? parseInt(values.headSupervisorId)
          : null,
        modemId: !!values.modemId ? parseInt(values.modemId) : null,
      });
    },
    [form, nationalSocietyId, edit],
  );

  if (!data || !form || isFetching) {
    return <Loading />;
  }

  return (
    <Fragment>
      {error && <ValidationMessage message={error.message} />}

      <Form onSubmit={handleSubmit}>
        <Grid container spacing={2}>
          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.common.email)}
              name="email"
              field={form.fields.email}
            />
          </Grid>

          <Grid item xs={12}>
            <TextInputField
              label={strings(stringKeys.common.name)}
              name="name"
              field={form.fields.name}
            />
          </Grid>

          <Grid item xs={12}>
            <PhoneInputField
              label={strings(stringKeys.nationalSocietyUser.form.phoneNumber)}
              name="phoneNumber"
              field={form.fields.phoneNumber}
              defaultCountry={countryCode}
              rtl={directionRtl}
            />
          </Grid>

          <Grid item xs={12}>
            <PhoneInputField
              label={strings(
                stringKeys.nationalSocietyUser.form.additionalPhoneNumber,
              )}
              name="additionalPhoneNumber"
              field={form.fields.additionalPhoneNumber}
              defaultCountry={countryCode}
              rtl={directionRtl}
            />
          </Grid>

          {canChangeOrganization && (
            <Grid item xs={12}>
              <SelectField
                label={strings(
                  stringKeys.nationalSocietyUser.form.organization,
                )}
                field={form.fields.organizationId}
                name="organizationId"
              >
                {organizations.map((organization) => (
                  <MenuItem
                    key={`organization_${organization.id}`}
                    value={organization.id.toString()}
                  >
                    {organization.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}

          {selectedRole !== roles.Coordinator && (
            <Grid item xs={12}>
              <TextInputField
                label={strings(
                  stringKeys.nationalSocietyUser.form.customOrganization,
                )}
                name="organization"
                field={form.fields.organization}
              />
            </Grid>
          )}

          {(selectedRole === roles.Supervisor ||
            selectedRole === roles.HeadSupervisor) && (
            <Fragment>
              <Grid item xs={12}>
                <SelectField
                  label={strings(
                    stringKeys.nationalSocietyUser.form.decadeOfBirth,
                  )}
                  field={form.fields.decadeOfBirth}
                  name="decadeOfBirth"
                >
                  {birthDecades.map((decade) => (
                    <MenuItem key={`birthDecade_${decade}`} value={decade}>
                      {parseBirthDecade(decade)}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.nationalSocietyUser.form.sex)}
                  field={form.fields.sex}
                  name="sex"
                >
                  {sexValues.map((type) => (
                    <MenuItem key={`sex${type}`} value={type}>
                      {strings(
                        stringKeys.dataCollectors.constants.sex[
                          type.toLowerCase()
                        ],
                      )}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>

              <Grid item xs={12}>
                <SelectField
                  label={strings(stringKeys.nationalSocietyUser.form.project)}
                  field={form.fields.projectId}
                  name="projectId"
                >
                  {data.editSupervisorFormData.availableProjects.map(
                    (project) => (
                      <MenuItem
                        key={`project_${project.id}`}
                        value={project.id.toString()}
                      >
                        {project.isClosed
                          ? stringsFormat(
                              stringKeys.nationalSocietyUser.form
                                .projectIsClosed,
                              { projectName: project.name },
                            )
                          : project.name}
                      </MenuItem>
                    ),
                  )}
                </SelectField>
              </Grid>
            </Fragment>
          )}

          {selectedRole === roles.Supervisor &&
            data.editSupervisorFormData.headSupervisors.length > 0 && (
              <Grid item xs={12}>
                <SelectField
                  label={strings(
                    stringKeys.nationalSocietyUser.form.headSupervisor,
                  )}
                  field={form.fields.headSupervisorId}
                  name="headSupervisorId"
                >
                  {!!data.headSupervisorId && (
                    <MenuItem value={"0"}>
                      {strings(
                        stringKeys.nationalSocietyUser.form
                          .headSupervisorNotAssigned,
                      )}
                    </MenuItem>
                  )}
                  {data.editSupervisorFormData.headSupervisors.map(
                    (headSupervisor) => (
                      <MenuItem
                        key={`headSupervisor_${headSupervisor.id}`}
                        value={headSupervisor.id.toString()}
                      >
                        {headSupervisor.name}
                      </MenuItem>
                    ),
                  )}
                </SelectField>
              </Grid>
            )}

          {canSelectModem && (
            <Grid item xs={12}>
              <SelectField
                label={strings(stringKeys.nationalSocietyUser.form.modem)}
                field={form.fields.modemId}
                name="modemId"
              >
                {modems.map((modem) => (
                  <MenuItem
                    key={`modemId_${modem.id}`}
                    value={modem.id.toString()}
                  >
                    {modem.name}
                  </MenuItem>
                ))}
              </SelectField>
            </Grid>
          )}
        </Grid>
        <FormActions>
          <CancelButton onClick={() => goToList(nationalSocietyId)}>
            {strings(stringKeys.form.cancel)}
          </CancelButton>
          <SubmitButton isFetching={isSaving}>
            {strings(stringKeys.common.buttons.update)}
          </SubmitButton>
        </FormActions>
      </Form>
    </Fragment>
  );
};

NationalSocietyUsersEditPageComponent.propTypes = {};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyUserId: ownProps.match.params.nationalSocietyUserId,
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  organizations: state.nationalSocietyUsers.formOrganizations,
  isFetching: state.nationalSocietyUsers.formFetching,
  isSaving: state.nationalSocietyUsers.formSaving,
  data: state.nationalSocietyUsers.formData,
  callingUserRoles: state.appData.user.roles,
  error: state.nationalSocietyUsers.formError,
  modems: state.nationalSocietyUsers.formModems,
  countryCode: state.nationalSocietyUsers.countryCode,
  directionRtl: state.appData.user.languageCode === "ar",
});

const mapDispatchToProps = {
  openEdition: nationalSocietyUsersActions.openEdition.invoke,
  edit: nationalSocietyUsersActions.edit.invoke,
  goToList: nationalSocietyUsersActions.goToList,
};

export const NationalSocietyUsersEditPage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(NationalSocietyUsersEditPageComponent),
);
