import type {
  RouteLocationNormalized
} from 'vue-router'

const updatePageTitle = (title: string) => {
  document.title = title
}

export const afterEach = (to: RouteLocationNormalized) => {
  if (to.meta?.title) {
    updatePageTitle(`Datenraum - ${to.meta.title}`)
  }
}
