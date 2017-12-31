FROM node:latest

WORKDIR /src
COPY package-lock.json package.json ./
RUN npm install --no-progress --ignore-optional

CMD npm run start:dev