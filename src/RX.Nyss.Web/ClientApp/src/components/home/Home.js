import React from 'react';
import { Layout } from '../layout/Layout';
import { wrapLayout } from '../../utils/layout';
import Typography from '@material-ui/core/Typography';

const HomeComponent = () => {
    return (
        <div>
            <Typography variant="h2">Dashboard</Typography>
        </div>
    );
}

export const Home = wrapLayout(Layout, HomeComponent);
