import React, { useEffect } from 'react';
import PropTypes from "prop-types";
import { connect } from "react-redux";
import { Layout } from '../../layout/Layout';
import { useLayout } from '../../../utils/layout';
import Typography from '@material-ui/core/Typography';
import * as nationalSocietiesActions from '../logic/nationalSocietiesActions';
import * as appActions from '../../app/logic/appActions';
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import Button from '@material-ui/core/Button';
import AddIcon from '@material-ui/icons/Add';
import ClearIcon from '@material-ui/icons/Clear';
import EditIcon from '@material-ui/icons/Edit';
import { TableActions } from '../../common/tableActions/TableActions';
import dayjs from "dayjs"
import { TableRowAction } from '../../common/tableRowAction/TableRowAction';

const NationalSocietiesListPageComponent = (props) => {
  useEffect(() => {
    props.getList()
    props.updateSiteMap(props.match.path, {})
  }, [])

  return (
    <div>
      <Typography variant="h2">National Societies</Typography>

      <TableActions>
        <Button variant="outlined" color="primary" startIcon={<AddIcon />}>
          Add national society
       </Button>
      </TableActions>

      <Table>
        <TableHead>
          <TableRow>
            <TableCell>Name</TableCell>
            <TableCell style={{ width: 140 }}>Country</TableCell>
            <TableCell style={{ width: 80 }}>Start date</TableCell>
            <TableCell style={{ width: 130 }}>Data owner</TableCell>
            <TableCell style={{ width: 130 }}>Technical advisor</TableCell>
            <TableCell style={{ width: 100 }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {props.list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>{row.name}</TableCell>
              <TableCell>{row.country}</TableCell>
              <TableCell>{dayjs(row.startDate).format("YYYY-MM-DD")}</TableCell>
              <TableCell>{row.dataOwner}</TableCell>
              <TableCell>{row.technicalAdvisor}</TableCell>
              <TableCell style={{ textAlign: "right", paddingTop: 0, paddingBottom: 0 }}>
                <TableRowAction icon={<EditIcon />} title={"Edit"} />
                <TableRowAction icon={<ClearIcon />} title={"Delete"} />
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </div>
  );
}

NationalSocietiesListPageComponent.propTypes = {
  getNationalSocieties: PropTypes.func,
  updateSiteMap: PropTypes.func,
  list: PropTypes.array
};

const mapStateToProps = state => ({
  list: state.nationalSocieties.list.data
});

const mapDispatchToProps = {
  getList: nationalSocietiesActions.getList.invoke,
  updateSiteMap: appActions.updateSiteMap
};

export const NationalSocietiesListPage = useLayout(
  Layout,
  connect(mapStateToProps, mapDispatchToProps)(NationalSocietiesListPageComponent)
);
