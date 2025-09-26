import axiosInstance from './axios'

import {
  AuthApiClient,
  UsersApiClient,
  RolesApiClient,
  SkillsApiClient,
} from '@/api/codegen'

let BASE_PATH = undefined
let configFile = undefined

try {
  const request = new XMLHttpRequest();
  request.open('GET', './config/env.json', false);
  request.send();

  configFile = JSON.parse(request.responseText).BASE_PATH;

  BASE_PATH = configFile ?? import.meta.env.VITE_API_URI;
} catch (e) {
  console.error(e)
}

BASE_PATH = configFile ?? import.meta.env.VITE_API_URI;

/* API CLIENTS */

const authApi = new AuthApiClient(BASE_PATH, axiosInstance);
const userApi = new UsersApiClient(BASE_PATH, axiosInstance);
const roleApi = new RolesApiClient(BASE_PATH, axiosInstance);
const skillApi = new SkillsApiClient(BASE_PATH, axiosInstance);

export {
  authApi,
  userApi,
  roleApi,
  skillApi,
}
