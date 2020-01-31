import React from 'react';
import PropTypes from "prop-types";
import Table from '@material-ui/core/Table';
import TableBody from '@material-ui/core/TableBody';
import TableCell from '@material-ui/core/TableCell';
import TableHead from '@material-ui/core/TableHead';
import TableRow from '@material-ui/core/TableRow';
import { Loading } from '../common/loading/Loading';
import { strings, stringKeys } from '../../strings';
import { TableContainer } from '../common/table/TableContainer';
import { StringsEditor } from '../common/stringsEditor/StringsEditor';

export const TranslationsTable = ({ isListFetching, list }) => {
  if (!list || !list.languages || !list.translations || isListFetching) {
    return <Loading />;
  }

  return (
    <TableContainer sticky>
      <Table>
        <TableHead>
          <TableRow>
            <TableCell>{strings(stringKeys.translations.list.key)}</TableCell>
            {list.languages.map(language => (
              <TableCell key={language.languageCode}>{language.displayName}</TableCell>
            ))}
          </TableRow>
        </TableHead>
        <TableBody>
          {list.translations.sort((a, b) => a.key > b.key ? 1 : -1).map(translation => (
            <TableRow key={translation.key}>
              <TableCell>
                <StringsEditor stringKey={translation.key} />
              </TableCell>
              {list.languages.map(language => (
                <TableCell key={`translation_${translation.key}_${language.languageCode}`}>
                  {translation.translations[language.languageCode]}
                </TableCell>
              ))}
            </TableRow>
          ))}
        </TableBody>
      </Table>
    </TableContainer>
  );
}

TranslationsTable.propTypes = {
  isFetching: PropTypes.bool,
  //list: PropTypes.object
};

export default TranslationsTable;