FROM node:20-alpine AS dev
WORKDIR /app

COPY apps/web/package*.json ./
RUN npm install

COPY apps/web/. .
EXPOSE 5173
CMD ["npm", "run", "dev", "--", "--host", "0.0.0.0"]

FROM node:20-alpine AS build
WORKDIR /app

ARG VITE_GOOGLE_CLIENT_ID
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID

COPY apps/web/package*.json ./
RUN npm install

COPY apps/web/. .
RUN npm run build

FROM nginx:1.27-alpine AS final
COPY docker/web/nginx.conf /etc/nginx/conf.d/default.conf
COPY --from=build /app/dist /usr/share/nginx/html
EXPOSE 80
