import React from 'react';
import { stringKeys } from '../../strings';
import { BaseLayout } from './BaseLayout';

export const NotFoundPage = () =>
  <BaseLayout authError={stringKeys.error.errorPage.notFound} />;

