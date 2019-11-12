import React from 'react';
import SnackbarContent from '@material-ui/core/SnackbarContent';
import { resetPageContentScroll } from '../layout/Layout';

export const ValidationMessage = ({ message }) => (
  <SnackbarContent
    message={message}
    ref={resetPageContentScroll}
  />
);