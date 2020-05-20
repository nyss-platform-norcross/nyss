import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import ClearIcon from '@material-ui/icons/Clear';
import { TableRowAction } from '../common/tableRowAction/TableRowAction';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { TableRowActions } from '../common/tableRowAction/TableRowActions';
import { accessMap } from '../../authentication/accessMap';

export const ProjectOrganizationsTable = ({ isListFetching, isRemoving, remove, list, projectId }) => {
  if (isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer sticky>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.projectOrganization.list.name)}</TableCell>
            <TableCell style={{ width: "10%" }} />
          </TableRow>
        </TableHead>
        <TableBody>
          {list.map(row => (
            <TableRow key={row.id} hover>
              <TableCell>{row.name}</TableCell>
              <TableCell>
                <TableRowActions>
                  <TableRowAction
                    roles={accessMap.projectOrganizations.delete}
                    onClick={() => remove(projectId, row.id)}
                    confirmationText={strings(stringKeys.projectOrganization.list.removalConfirmation)}
                    icon={<ClearIcon />}
                    title={"Delete"}
                    isFetching={isRemoving[row.id]} />
                </TableRowActions>
              </TableCell>
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

ProjectOrganizationsTable.propTypes = {
  isFetching: PropTypes.bool,
  list: PropTypes.array
};

export default ProjectOrganizationsTable;