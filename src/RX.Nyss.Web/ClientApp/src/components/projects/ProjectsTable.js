import React from 'react';
import PropTypes from "prop-types";
import Grid from '@material-ui/core/Grid';
import Card from '@material-ui/core/Card';
import CardActions from '@material-ui/core/CardActions';
import CardContent from '@material-ui/core/CardContent';
import Typography from '@material-ui/core/Typography';
import Container from '@material-ui/core/Container';
import Button from '@material-ui/core/Button';
import IconButton from '@material-ui/core/IconButton';
import ClearIcon from '@material-ui/icons/Clear';
import EditIcon from '@material-ui/icons/Edit';
import ArrowForwardIcon from '@material-ui/icons/ArrowForward';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';

export const ProjectsTable = ({ isListFetching, isRemoving, goToEdition, remove, list, nationalSocietyId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <Container maxWidth="sm">
      {list.map(row => (
        <Card key={row.id}>
          <CardContent>
            <Typography variant="h3" gutterBottom>
              {row.name}
            </Typography>
            <Typography gutterBottom variant="body1" color="textSecondary" gutterBottom>
              {row.state}
            </Typography>            
            <Grid container spacing={2}>
              <Grid item xs={12} sm container>
                <Grid item xs container direction="column" spacing={2}>
                  <Grid item xs>
                    <Typography variant="subtitle1">
                      {row.totalReportCount}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      {stringKeys.project.list.totalReportCount}
                    </Typography>
                    <Typography variant="subtitle1">
                      {row.activeDataCollectorCount}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      {stringKeys.project.list.activeDataCollectorCount}
                    </Typography>
                    <Typography variant="subtitle1">
                      {row.startDate}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      {stringKeys.project.list.startDate}
                    </Typography>
                  </Grid>
                </Grid>
                <Grid item xs container direction="column" spacing={2}>
                  <Grid item xs>
                    <Typography variant="subtitle1">
                      {row.escalatedAlertCount}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      {stringKeys.project.list.escalatedAlertCount}
                    </Typography>
                    <Typography variant="subtitle1">
                      {row.supervisorCount}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      {stringKeys.project.list.supervisorCount}
                    </Typography>
                    <Typography variant="subtitle1">
                      {row.endDate ? row.endDate : stringKeys.project.list.ongoing}
                    </Typography>
                    <Typography variant="body2" gutterBottom>
                      {stringKeys.project.list.endDate}
                    </Typography>
                  </Grid>
                </Grid>
              </Grid>
            </Grid>           
          </CardContent>
          <CardActions>
            <IconButton size="small" onClick={() => goToEdition(nationalSocietyId, row.id)} aria-label={"Edit"}><EditIcon /></IconButton>
            <IconButton size="small" onClick={() => remove(nationalSocietyId, row.id)} confirmationText={strings(stringKeys.project.list.confirmationText)} aria-label={"Delete"} isFetching={isRemoving[row.id]}><ClearIcon /></IconButton>
            <Button onClick={() => goToEdition(nationalSocietyId, row.id)} endIcon={<ArrowForwardIcon />}>{strings(stringKeys.project.list.open)}</Button>
          </CardActions>
        </Card>
      ))}
    </Container>
  );
}

ProjectsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ProjectsTable;