FROM node:current-alpine AS build
WORKDIR /app

COPY ./musicx-site/ .

RUN npm install
RUN npm run build

FROM busybox

RUN adduser -D static
USER static
WORKDIR /home/static

COPY --from=build /app/build .

# Run BusyBox httpd
CMD ["busybox", "httpd", "-f", "-v", "-p", "8080"]