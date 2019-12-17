import React from 'react';
import SnackbarContent from '@material-ui/core/SnackbarContent';
import { resetPageContentScroll } from '../layout/Layout';
import { extractString } from '../../strings';

export const ValidationMessage = ({ message }) => (
  <SnackbarContent
    message={extractString(message)}
    ref={resetPageContentScroll}
  />
);