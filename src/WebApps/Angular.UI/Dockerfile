# Stage 1: Build Angular application
FROM node:22-alpine AS build
WORKDIR /app

# Copy package files and install dependencies
COPY package*.json ./
RUN npm ci

# Copy the rest of the application files
COPY . .

# Build the Angular application
RUN npm run build -- --configuration=production

# Stage 2: Serve the application with Nginx
FROM nginx:alpine
COPY --from=build /app/dist/Angular.UI/browser /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80
EXPOSE 81


CMD ["nginx", "-g", "daemon off;"]
