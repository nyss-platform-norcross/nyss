import React, {
  useState,
  Fragment,
  useMemo,
  useCallback,
  useEffect,
} from "react";
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
import SelectInput from "../forms/SelectField";
import PhoneInputField from "../forms/PhoneInputField";
import { MenuItem, Grid } from "@material-ui/core";
import { useMount } from "../../utils/lifecycle";
import { strings, stringKeys } from "../../strings";
import {
  userRoles,
  globalCoordinatorUserRoles,
  coordinatorUserRoles,
  headManagerRoles,
  sexValues,
} from "./logic/nationalSocietyUsersConstants";
import * as roles from "../../authentication/roles";
import SelectField from "../forms/SelectField";
import { ValidationMessage } from "../forms/ValidationMessage";
import { ConfirmationDialog } from "../common/confirmationDialog/ConfirmationDialog";
import { getBirthDecades, parseBirthDecade } from "../../utils/birthYear";
import { trackPageView } from "../../utils/appInsightsHelper";

const NationalSocietyUsersCreatePageComponent = ({
  nationalSocietyId,
  data,
  isSaving,
  error,
  callingUserRoles,
  directionRtl,
  openCreation,
  create,
  goToList,
}) => {
  const [birthDecades] = useState(getBirthDecades());
  const [selectedRole, setRole] = useState(null);
  const [projects, setProjects] = useState([]);
  const [confirmCoordinatorDialog, setConfirmCoordinatorDialog] = useState({
    isOpened: false,
    isConfirmed: false,
  });

  useMount(() => {
    openCreation(nationalSocietyId);

    // Track page view
    trackPageView("NationalSocietyUsersCreatePage");
  });

  const hasAnyRole = useCallback(
    (...roles) =>
      callingUserRoles.some((userRole) =>
        roles.some((role) => role === userRole),
      ),
    [callingUserRoles],
  );

  const canChangeOrganization = useMemo(
    () =>
      (hasAnyRole(roles.Administrator, roles.Coordinator) &&
        selectedRole !== roles.DataConsumer) ||
      (hasAnyRole(roles.GlobalCoordinator) &&
        selectedRole === roles.Coordinator) ||
      (data &&
        data.isHeadManager &&
        !data.hasCoordinator &&
        selectedRole === roles.Coordinator),
    [hasAnyRole, selectedRole, data],
  );

  const canSelectModem = useMemo(
    () =>
      (selectedRole === roles.Manager ||
        selectedRole === roles.TechnicalAdvisor ||
        selectedRole === roles.HeadSupervisor ||
        selectedRole === roles.Supervisor) &&
      data &&
      data.modems.length > 0,
    [data, selectedRole],
  );

  const availableUserRoles = useMemo(() => {
    if (!data) {
      return [];
    }

    if (callingUserRoles.some((r) => r === roles.Administrator)) {
      return headManagerRoles;
    }

    if (hasAnyRole(roles.GlobalCoordinator)) {
      return globalCoordinatorUserRoles.filter(
        (r) => !data.hasCoordinator || r !== roles.Coordinator,
      );
    }

    if (hasAnyRole(roles.Coordinator)) {
      if (data.organizations.every((o) => o.hasHeadManager)) {
        return [roles.Coordinator];
      }

      return coordinatorUserRoles;
    }

    if (data.isHeadManager) {
      return headManagerRoles.filter(
        (r) => !data.hasCoordinator || r !== roles.Coordinator,
      );
    }

    return userRoles;
  }, [hasAnyRole, callingUserRoles, data]);

  const availableOrganizations = useMemo(() => {
    if (!data) {
      return [];
    }

    if (hasAnyRole(roles.Coordinator) && selectedRole === roles.Manager) {
      return data.organizations.filter((o) => !o.hasHeadManager);
    }

    return data.organizations;
  }, [hasAnyRole, data, selectedRole]);

  const form = useMemo(() => {
    const fields = {
      nationalSocietyId: parseInt(nationalSocietyId),
      role: "",
      name: "",
      email: "",
      phoneNumber: "",
      additionalPhoneNumber: "",
      organization: "",
      decadeOfBirth: "",
      projectId: "",
      sex: "",
      organizationId: "",
      headSupervisorId: "",
      modemId: "",
    };

    const validation = {
      role: [validators.required],
      name: [validators.required, validators.maxLength(100)],
      email: [validators.required, validators.email, validators.maxLength(100)],
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
    };

    const newForm = createForm(fields, validation);
    newForm.fields.role.subscribe(({ newValue }) => setRole(newValue));

    return newForm;
  }, [nationalSocietyId]);

  useEffect(() => {
    if (!form || selectedRole) {
      return;
    }

    const newRole = availableUserRoles.some((r) => r === roles.Manager)
      ? roles.Manager
      : availableUserRoles[0];
    setRole(newRole);
    form.fields.role.update(newRole);
  }, [availableUserRoles, selectedRole, form]);

  useEffect(() => {
    form &&
      form.fields.organizationId.setValidators([
        validators.requiredWhen((_) => canChangeOrganization),
      ]);
  }, [form, canChangeOrganization]);

  useEffect(() => {
    form &&
      form.fields.modemId.setValidators([
        validators.requiredWhen((_) => canSelectModem),
      ]);
  }, [form, canSelectModem]);

  useEffect(() => {
    if (data == null) {
      return;
    }
    if (data.projects.length > 0) {
      setProjects(data.projects);
    } else {
      setProjects([
        {
          id: 0,
          name: strings(stringKeys.nationalSocietyUser.form.projectIsMissing),
        },
      ]);
    }
  }, [data]);

  useEffect(() => {
    if (!form) {
      return;
    }

    const organizationId = availableOrganizations.some(
      (o) => o.isDefaultOrganization,
    )
      ? availableOrganizations
          .filter((o) => o.isDefaultOrganization)[0]
          .id.toString()
      : (availableOrganizations.length > 0 &&
          availableOrganizations[0].id.toString()) ||
        "";

    form.fields.organizationId.update(organizationId);
  }, [availableOrganizations, availableUserRoles, form]);

  useCustomErrors(form, error);

  const createUser = useCallback(() => {
    const values = form.getValues();
    create(nationalSocietyId, {
      ...values,
      organizationId:
        canChangeOrganization && values.organizationId
          ? parseInt(values.organizationId)
          : null,
      projectId: values.projectId ? parseInt(values.projectId) : null,
      decadeOfBirth: values.decadeOfBirth
        ? parseInt(values.decadeOfBirth)
        : null,
      setAsHeadManager: hasAnyRole(roles.Coordinator, roles.GlobalCoordinator)
        ? true
        : null,
      headSupervisorId: values.headSupervisorId
        ? parseInt(values.headSupervisorId)
        : null,
      modemId: !!values.modemId ? parseInt(values.modemId) : null,
    });
  }, [hasAnyRole, canChangeOrganization, form, create, nationalSocietyId]);

  const handleSubmit = useCallback(
    (e) => {
      e.preventDefault();

      if (!form.isValid()) {
        return;
      }

      if (
        selectedRole === roles.Coordinator &&
        !data.hasCoordinator &&
        confirmCoordinatorDialog.isConfirmed === false
      ) {
        setConfirmCoordinatorDialog({
          ...confirmCoordinatorDialog,
          isOpened: true,
        });
        return;
      }

      createUser();
    },
    [createUser, form, selectedRole, data, confirmCoordinatorDialog],
  );

  if (!data) {
    return null;
  }

  const confirmCoordinatorCreation = () => {
    setConfirmCoordinatorDialog({
      ...confirmCoordinatorDialog,
      isConfirmed: true,
      isOpened: false,
    });
    createUser();
  };

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
              autoFocus
              inputMode={"email"}
            />
          </Grid>

          <Grid item xs={12}>
            <SelectInput
              label={strings(stringKeys.nationalSocietyUser.form.role)}
              name="role"
              field={form.fields.role}
            >
              {availableUserRoles.map((role) => (
                <MenuItem key={`role${role}`} value={role}>
                  {strings(
                    `role.${(hasAnyRole(
                      roles.Coordinator,
                      roles.GlobalCoordinator,
                    ) && role === roles.Manager
                      ? "headManager"
                      : role
                    ).toLowerCase()}`,
                  )}
                </MenuItem>
              ))}
            </SelectInput>
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
              defaultCountry={data.countryCode}
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
              defaultCountry={data.countryCode}
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
                customProps={{
                  disabled:
                    selectedRole === roles.Coordinator &&
                    hasAnyRole(roles.GlobalCoordinator),
                }}
              >
                {availableOrganizations.map((organization) => (
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
                  {projects.map((project) => (
                    <MenuItem
                      key={`project_${project.id}`}
                      value={project.id.toString()}
                      disabled={project.id === 0}
                    >
                      {project.name}
                    </MenuItem>
                  ))}
                </SelectField>
              </Grid>
            </Fragment>
          )}

          {selectedRole === roles.Supervisor &&
            data.headSupervisors.length > 0 && (
              <Grid item xs={12}>
                <SelectField
                  label={strings(
                    stringKeys.nationalSocietyUser.form.headSupervisor,
                  )}
                  field={form.fields.headSupervisorId}
                  name="headSupervisorId"
                >
                  {data.headSupervisors.map((headSupervisor) => (
                    <MenuItem
                      key={`headSupervisor_${headSupervisor.id}`}
                      value={headSupervisor.id.toString()}
                    >
                      {headSupervisor.name}
                    </MenuItem>
                  ))}
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
                {data.modems.map((modem) => (
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
            {strings(stringKeys.common.buttons.add)}
          </SubmitButton>
        </FormActions>
      </Form>

      <ConfirmationDialog
        isOpened={confirmCoordinatorDialog.isOpened}
        titleText={strings(
          stringKeys.nationalSocietyUser.form.confirmCoordinatorCreation,
        )}
        submit={() => confirmCoordinatorCreation(handleSubmit)}
        close={() =>
          setConfirmCoordinatorDialog({
            ...confirmCoordinatorDialog,
            isOpened: false,
          })
        }
        contentText={strings(
          stringKeys.nationalSocietyUser.form.confirmCoordinatorCreationText,
        )}
      />
    </Fragment>
  );
};

const mapStateToProps = (state, ownProps) => ({
  nationalSocietyId: ownProps.match.params.nationalSocietyId,
  data: state.nationalSocietyUsers.formAdditionalData,
  isSaving: state.nationalSocietyUsers.formSaving,
  error: state.nationalSocietyUsers.formError,
  callingUserRoles: state.appData.user.roles,
  directionRtl: state.appData.user.languageCode === "ar",
});

const mapDispatchToProps = {
  openCreation: nationalSocietyUsersActions.openCreation.invoke,
  create: nationalSocietyUsersActions.create.invoke,
  goToList: nationalSocietyUsersActions.goToList,
};

export const NationalSocietyUsersCreatePage = withLayout(
  Layout,
  connect(
    mapStateToProps,
    mapDispatchToProps,
  )(NationalSocietyUsersCreatePageComponent),
);
