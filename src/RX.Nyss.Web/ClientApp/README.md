# RX.Nyss.Web<span></span> UI project

## Used technologies

The UI project to run uses libraries:
- Create React App
- React
- Redux
- Redux Saga
- Material UI

## Running the UI project separately from the .NET application

The UI application can be run automatically with the back-end .NET solution, which requires no work, but it's also possible to run the UI part separately. It might speed up the UI development and to not be tight to the .NET build pipline when doing changes in the code.

### Preparation

To prepare the UI application to run make sure that the Node (version 10 or newer) or npm (version 6 or newer) are installed.

First step is to install the required npm packages. In order to do it go to folder `src/RX.Nyss.Web/ClientApp` and run:

```
npm install
```

### Running the project

In order to run the project, go to foloder `src/RX.Nyss.Web/ClientApp` and run the following command:

```
npm run start
```

The application will start on port 3000
