import { InputLabel, Grid, Divider, Chip } from "@material-ui/core";
import { makeStyles } from "@material-ui/core/styles";
import { connect } from "react-redux";
import { strings, stringKeys } from "../../strings";
import Card from "@material-ui/core/Card";
import CardContent from "@material-ui/core/CardContent";
import Typography from "@material-ui/core/Typography";
import TrendingFlatIcon from "@material-ui/icons/TrendingFlat";

const useStyles = makeStyles((theme) => ({
  inputLabel: {
    fontSize: 24,
    fontWeight: 700,
    marginBottom: 50,
  },
  card: {
    width: "80%",
    border: "none",
    boxShadow: "0px 1px 2px 0px rgba(0, 0, 0, 0.25)",
  },
  cardContent: {
    padding: 50,
  },
  divider: {
    margin: "30px 0 30px 0",
  },
  healthRisk: {
    margin: "4px 4px 4px 0",
  },
  healthRiskBackground: {
    backgroundColor: "#CDDDE7",
  },
  newLocation: {
    fontWeight: 700,
    color: theme.palette.primary.main,
  },
  arrow: {
    margin: "0 8px 0 8px",
  },
}));

const SummaryRow = ({ name, value }) => {
  const classes = useStyles();
  return (
    <>
      <Grid container>
        <Grid item xs={4}>
          <Typography>{name}</Typography>
        </Grid>
        <Grid item xs={8}>
          <Typography style={{ fontWeight: 700 }}>{value}</Typography>
        </Grid>
      </Grid>
      <Divider className={classes.divider} />
    </>
  );
};
const SummaryHealthRisksRow = ({ name, healthRisks }) => {
  const classes = useStyles();
  return (
    <>
      <Grid container alignItems="center">
        <Grid item xs={4}>
          <Typography>{name}</Typography>
        </Grid>
        <Grid item xs={8}>
          {healthRisks.map((hr) => (
            <Chip
              label={hr.healthRiskName}
              className={`${classes.healthRisk} ${
                hr.healthRiskType !== "Activity" && classes.healthRiskBackground
              } `}
              key={hr.healthRiskId}
            />
          ))}
        </Grid>
      </Grid>
      <Divider className={classes.divider} />
    </>
  );
};

const SummaryGeographicalStructureRow = ({ name, rows }) => {
  const classes = useStyles();
  return (
    <>
      <Grid container>
        <Grid item xs={4}>
          <Typography>{name}</Typography>
        </Grid>
        <Grid item xs={8}>
          {rows.length > 0 ? (
            rows.map((row, index) => {
              return (
                <Grid
                  item
                  container
                  direction="row"
                  xs={12}
                  alignItems="center"
                  key={`${row.region.id}_${index}`}
                >
                  <Typography
                    className={row.region.canModify && classes.newLocation}
                  >
                    {row.region.name}
                  </Typography>
                  {row.district && (
                    <TrendingFlatIcon className={classes.arrow} />
                  )}
                  {row.district && (
                    <>
                      <Typography
                        className={
                          row.district.canModify && classes.newLocation
                        }
                      >
                        {row.district.name}
                      </Typography>
                      {row.village && (
                        <TrendingFlatIcon
                          fontSize="small"
                          className={classes.arrow}
                        />
                      )}
                    </>
                  )}
                  {row.village && (
                    <>
                      <Typography
                        className={row.village.canModify && classes.newLocation}
                      >
                        {row.village.name}
                      </Typography>
                      {row.zone && (
                        <TrendingFlatIcon
                          fontSize="small"
                          className={classes.arrow}
                        />
                      )}
                    </>
                  )}
                  {row.zone && (
                    <>
                      <Typography
                        className={row.zone.canModify && classes.newLocation}
                      >
                        {row.zone.name}
                      </Typography>
                    </>
                  )}
                </Grid>
              );
            })
          ) : (
            <Typography style={{ fontWeight: 700 }}>
              {strings(stringKeys.projectSetup.summary.notUpdated)}
            </Typography>
          )}
        </Grid>
      </Grid>
    </>
  );
};

const ProjectSetupSummaryComponent = (props) => {
  let {
    projectName,
    organizations,
    organizationId,
    recipients,
    recipientIds,
    healthRisks,
    requiredHealthRisks,
    regions,
    districts,
    villages,
    zones,
  } = props;
  const classes = useStyles();
  const organizationName = organizations?.find(
    (org) => org.id === organizationId,
  ).name;
  const selectedRecipients = recipients
    ?.filter((recipient) => recipientIds.includes(recipient.id))
    .map((recipient) => recipient.name);

  let allLocationRows = [];
  regions.forEach((region) =>
    allLocationRows.push({
      region: region,
      districts: [],
      villages: [],
      zones: [],
    }),
  );

  districts.forEach((district) => {
    allLocationRows.forEach((row) => {
      if (district.regionId === row.region?.id) {
        row.districts = [...row.districts, district];
      }
    });
  });
  villages.forEach((village) => {
    allLocationRows.forEach((row) => {
      const district = row.districts.find(
        (district) => district.id === village.districtId,
      );
      if (district) {
        row.villages = [...row.villages, village];
      }
    });
  });
  zones.forEach((zone) => {
    allLocationRows.forEach((row) => {
      const village = row.villages.find(
        (village) => village.id === zone.villageId,
      );
      if (village) {
        row.zones = [...row.zones, zone];
      }
    });
  });

  const newLocationRows = [];
  allLocationRows.forEach((row) => {
    const newZones = row.zones.filter((district) => district.canModify);
    if (newZones.length > 0) {
      newZones.forEach((zone) => {
        const village = row.villages.find(
          (village) => village.id === zone.villageId,
        );
        const district = row.districts.find(
          (district) => district.id === village.districtId,
        );
        newLocationRows.push({
          region: row.region,
          district: district,
          village: village,
          zone: zone,
        });
      });
    }
    const newVillages = row.villages.filter((village) => village.canModify);
    if (newVillages.length > 0) {
      newVillages.forEach((village) => {
        if (
          !newLocationRows.some(
            (location) => location.village?.id === village.id,
          )
        ) {
          const district = row.districts.find(
            (district) => district.id === village.districtId,
          );
          newLocationRows.push({
            region: row.region,
            district: district,
            village: village,
            zone: null,
          });
        }
      });
    }
    const newDistricts = row.districts.filter((district) => district.canModify);
    if (newDistricts.length > 0) {
      newDistricts.forEach((district) => {
        if (
          !newLocationRows.some(
            (location) => location.district?.id === district.id,
          )
        ) {
          newLocationRows.push({
            region: row.region,
            district: district,
            village: null,
            zone: null,
          });
        }
      });
    }
  });
  const newRegions = allLocationRows
    .filter((row) => row.region.canModify)
    .map((row) => row.region);
  if (newRegions.length > 0) {
    newRegions.forEach((region) => {
      if (
        newLocationRows.every((location) => location.region.id !== region.id)
      ) {
        newLocationRows.push({
          region: region,
          district: null,
          village: null,
          zone: null,
        });
      }
    });
  }

  return (
    <>
      <InputLabel className={classes.inputLabel}>
        {strings(stringKeys.projectSetup.summary.title)}
      </InputLabel>
      <Card className={classes.card} variant="elevation">
        <CardContent className={classes.cardContent}>
          <SummaryRow
            name={strings(stringKeys.projectSetup.projectName.name)}
            value={projectName}
          />
          <SummaryRow
            name={strings(stringKeys.projectSetup.projectOrganization.name)}
            value={organizationName}
          />
          <SummaryRow
            name={strings(stringKeys.projectSetup.projectRecipients.name)}
            value={selectedRecipients?.join(", ")}
          />
          <SummaryHealthRisksRow
            name={strings(stringKeys.projectSetup.projectHealthRisks.name)}
            healthRisks={healthRisks.concat(requiredHealthRisks)}
          />
          <SummaryGeographicalStructureRow
            name={strings(stringKeys.projectSetup.geographicalStructure.name)}
            rows={newLocationRows}
          />
        </CardContent>
      </Card>
    </>
  );
};

const mapStateToProps = (state) => ({
  projectName: state.projectSetup.projectName,
  organizationId: state.projectSetup.organizationId,
  recipients: state.projectSetup.formData?.alertNotHandledRecipients,
  organizations: state.projectSetup.formData?.organizations,
  recipientIds: state.projectSetup.alertNotHandledNotificationRecipientIds,
  healthRisks: state.projectSetup.healthRisks,
  requiredHealthRisks: state.projectSetup.requiredHealthRisks,
  regions: state.projectSetup.regions,
  districts: state.projectSetup.districts,
  villages: state.projectSetup.villages,
  zones: state.projectSetup.zones,
});

const mapDispatchToProps = {};

export const ProjectSetupSummary = connect(
  mapStateToProps,
  mapDispatchToProps,
)(ProjectSetupSummaryComponent);
