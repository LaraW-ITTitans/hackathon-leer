/* eslint-disable class-methods-use-this */
/* eslint-disable @typescript-eslint/no-unused-vars */
/* eslint-disable unicorn/no-abusive-eslint-disable */
// imported (but unsued) because they are qeuired by the nswag generator
import axios from 'axios';
import type {
  AxiosError,
  AxiosInstance,
  AxiosRequestConfig,
  AxiosResponse,
  CancelToken,
} from 'axios';

export default class AuthorizedApiBase {
  protected transformOptions = (options: AxiosRequestConfig): Promise<AxiosRequestConfig> => {
    const newOptions: AxiosRequestConfig = options;

    // needed to prevent double json parsing (axios vs nswag)
    newOptions.transformResponse = undefined;

    newOptions.withCredentials = true;

    return Promise.resolve(newOptions);
  };
}
